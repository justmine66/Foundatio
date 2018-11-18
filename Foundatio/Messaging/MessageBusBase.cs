using Foundatio.Queues;
using Foundatio.Serializer;
using Foundatio.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging
{
    public abstract class MessageBusBase<TOptions> : MaintenanceBase, IMessageBus, IMessagePublisher, IMessageSubscriber, IDisposable where TOptions : SharedMessageBusOptions
    {
        [DebuggerDisplay("MessageType: {MessageType} SendTime: {SendTime} Message: {Message}")]
        protected class DelayedMessage
        {
            public DateTime SendTime
            {
                get;
                set;
            }

            public Type MessageType
            {
                get;
                set;
            }

            public object Message
            {
                get;
                set;
            }
        }

        [DebuggerDisplay("Id: {Id} Type: {Type} CancellationToken: {CancellationToken}")]
        protected class Subscriber
        {
            private readonly ConcurrentDictionary<Type, bool> _assignableTypesCache = new ConcurrentDictionary<Type, bool>();

            public string Id
            {
                get;
                private set;
            } = Guid.NewGuid().ToString();


            public CancellationToken CancellationToken
            {
                get;
                set;
            }

            public Type Type
            {
                get;
                set;
            }

            public Func<object, CancellationToken, Task> Action
            {
                get;
                set;
            }

            public bool IsAssignableFrom(Type type)
            {
                return _assignableTypesCache.GetOrAdd(type, (Type t) => Type.GetTypeInfo().IsAssignableFrom(t));
            }
        }

        private readonly TaskQueue _queue;

        protected readonly ConcurrentDictionary<string, Subscriber> _subscribers = new ConcurrentDictionary<string, Subscriber>();

        private readonly ConcurrentDictionary<Guid, DelayedMessage> _delayedMessages = new ConcurrentDictionary<Guid, DelayedMessage>();

        protected readonly TOptions _options;

        protected readonly ISerializer _serializer;

        private readonly ConcurrentDictionary<Type, string> _mappedMessageTypesCache = new ConcurrentDictionary<Type, string>();

        private readonly ConcurrentDictionary<string, Type> _knownMessageTypesCache = new ConcurrentDictionary<string, Type>();

        public string MessageBusId
        {
            get;
            protected set;
        }

        public MessageBusBase(TOptions options)
            : base(options?.LoggerFactory)
        {
            _options = options ?? throw new ArgumentNullException("options");
            _serializer = (options.Serializer ?? DefaultSerializer.Instance);
            MessageBusId = _options.Topic + Guid.NewGuid().ToString("N").Substring(10);
            _queue = new TaskQueue(options.TaskQueueMaxItems, options.TaskQueueMaxDegreeOfParallelism, true, null, options.LoggerFactory);
            InitializeMaintenance(null, null);
        }

        protected virtual Task EnsureTopicCreatedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected abstract Task PublishImplAsync(string messageType, object message, TimeSpan? delay, CancellationToken cancellationToken);

        public async Task PublishAsync(Type messageType, object message, TimeSpan? delay = default(TimeSpan?), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(messageType == (Type)null) && message != null)
            {
                await this.EnsureTopicCreatedAsync(cancellationToken).AnyContext();
                await this.PublishImplAsync(this.GetMappedMessageType(messageType), message, delay, cancellationToken).AnyContext();
            }
        }

        protected string GetMappedMessageType(Type messageType)
        {
            return _mappedMessageTypesCache.GetOrAdd(messageType, delegate (Type type)
            {
                Dictionary<Type, string> dictionary = Enumerable.ToDictionary<KeyValuePair<string, Type>, Type, string>((IEnumerable<KeyValuePair<string, Type>>)_options.MessageTypeMappings, (Func<KeyValuePair<string, Type>, Type>)((KeyValuePair<string, Type> kvp) => kvp.Value), (Func<KeyValuePair<string, Type>, string>)((KeyValuePair<string, Type> kvp) => kvp.Key));
                if (dictionary.ContainsKey(type))
                {
                    return dictionary[type];
                }
                return messageType.FullName + ", " + messageType.Assembly.GetName().Name;
            });
        }

        protected Type GetMappedMessageType(string messageType)
        {
            return _knownMessageTypesCache.GetOrAdd(messageType, delegate (string type)
            {
                if (_options.MessageTypeMappings == null || !_options.MessageTypeMappings.ContainsKey(type))
                {
                    try
                    {
                        return Type.GetType(type);
                    }
                    catch (Exception ex)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                        {
                            LoggerExtensions.LogWarning(_logger, ex, "Error getting message body type: {MessageType}", new object[1]
                            {
                            type
                            });
                        }
                        return null;
                    }
                }
                return _options.MessageTypeMappings[type];
            });
        }

        protected virtual Task EnsureTopicSubscriptionAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task SubscribeImplAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken) where T : class
        {
            Subscriber subscriber = new Subscriber
            {
                CancellationToken = cancellationToken,
                Type = typeof(T),
                Action = (Func<object, CancellationToken, Task>)delegate (object message, CancellationToken token)
                {
                    if (!(message is T))
                    {
                        if (_logger.IsEnabled(0))
                        {
                            LoggerExtensions.LogTrace(_logger, "Unable to call subscriber action: {MessageType} cannot be safely casted to {SubscriberType}", new object[2]
                            {
                            message.GetType(),
                            typeof(T)
                            });
                        }
                        return Task.CompletedTask;
                    }
                    return handler((T)message, cancellationToken);
                }
            };
            if (!this._subscribers.TryAdd(subscriber.Id, subscriber) && _logger.IsEnabled(LogLevel.Error))
            {
                LoggerExtensions.LogError(_logger, "Unable to add subscriber {SubscriberId}", new object[1]
                {
                subscriber.Id
                });
            }
            return Task.CompletedTask;
        }

        public async Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            if (_logger.IsEnabled(0))
            {
                LoggerExtensions.LogTrace(_logger, "Adding subscriber for {MessageType}.", new object[1]
                {
                typeof(T).FullName
                });
            }
            await this.EnsureTopicSubscriptionAsync(cancellationToken).AnyContext();
            await SubscribeImplAsync(handler, cancellationToken).AnyContext();
        }

        protected void SendMessageToSubscribers(MessageBusData message, ISerializer serializer)
        {
            Type messageType = GetMessageBodyType(message);
            if (!(messageType == (Type)null))
            {
                List<Subscriber> list = Enumerable.ToList<Subscriber>(Enumerable.Where<Subscriber>((IEnumerable<Subscriber>)_subscribers.Values, (Func<Subscriber, bool>)((Subscriber s) => s.IsAssignableFrom(messageType))));
                if (list.Count == 0)
                {
                    if (_logger.IsEnabled(0))
                    {
                        LoggerExtensions.LogTrace(_logger, "Done sending message to 0 subscribers for message type {MessageType}.", new object[1]
                        {
                        messageType.Name
                        });
                    }
                }
                else
                {
                    object obj;
                    try
                    {
                        obj = serializer.Deserialize(message.Data, messageType);
                    }
                    catch (Exception ex)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                        {
                            LoggerExtensions.LogWarning(_logger, ex, "Error deserializing messsage body: {Message}", new object[1]
                            {
                            ex.Message
                            });
                        }
                        return;
                    }
                    if (obj == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                        {
                            LoggerExtensions.LogWarning(_logger, "Unable to send null message for type {MessageType}", new object[1]
                            {
                            messageType.Name
                            });
                        }
                    }
                    else
                    {
                        SendMessageToSubscribers(list, messageType, obj);
                    }
                }
            }
        }

        protected void SendMessageToSubscribers(List<Subscriber> subscribers, Type messageType, object message)
        {
            bool isTraceLogLevelEnabled = _logger.IsEnabled(0);
            if (isTraceLogLevelEnabled)
            {
                LoggerExtensions.LogTrace(_logger, "Found {SubscriberCount} subscribers for message type {MessageType}.", new object[2]
                {
                subscribers.Count,
                messageType.Name
                });
            }
            foreach (Subscriber subscriber in subscribers)
            {
                if (subscriber.CancellationToken.IsCancellationRequested)
                {
                    if (_subscribers.TryRemove(subscriber.Id, out Subscriber _))
                    {
                        if (isTraceLogLevelEnabled)
                        {
                            LoggerExtensions.LogTrace(_logger, "Removed cancelled subscriber: {SubscriberId}", new object[1]
                            {
                            subscriber.Id
                            });
                        }
                    }
                    else if (isTraceLogLevelEnabled)
                    {
                        LoggerExtensions.LogTrace(_logger, "Unable to remove cancelled subscriber: {SubscriberId}", new object[1]
                        {
                        subscriber.Id
                        });
                    }
                }
                else
                {
                    _queue.Enqueue(async delegate
                    {
                        if (subscriber.CancellationToken.IsCancellationRequested)
                        {
                            if (isTraceLogLevelEnabled)
                            {
                                LoggerExtensions.LogTrace(_logger, "The cancelled subscriber action will not be called: {SubscriberId}", new object[1]
                                {
                                subscriber.Id
                                });
                            }
                        }
                        else
                        {
                            if (isTraceLogLevelEnabled)
                            {
                                LoggerExtensions.LogTrace(_logger, "Calling subscriber action: {SubscriberId}", new object[1]
                                {
                                subscriber.Id
                                });
                            }
                            try
                            {
                                await subscriber.Action(message, subscriber.CancellationToken).AnyContext();
                                if (isTraceLogLevelEnabled)
                                {
                                    LoggerExtensions.LogTrace(_logger, "Finished calling subscriber action: {SubscriberId}", new object[1]
                                    {
                                    subscriber.Id
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                if (_logger.IsEnabled(LogLevel.Warning))
                                {
                                    LoggerExtensions.LogWarning(_logger, ex, "Error sending message to subscriber: {Message}", new object[1]
                                    {
                                    ex.Message
                                    });
                                }
                            }
                        }
                    });
                }
            }
            if (isTraceLogLevelEnabled)
            {
                LoggerExtensions.LogTrace(_logger, "Done enqueueing message to {SubscriberCount} subscribers for message type {MessageType}.", new object[2]
                {
                subscribers.Count,
                messageType.Name
                });
            }
        }

        protected Type GetMessageBodyType(MessageBusData message)
        {
            if (message == null || message.Type == null)
            {
                return null;
            }
            return GetMappedMessageType(message.Type);
        }

        protected Task AddDelayedMessageAsync(Type messageType, object message, TimeSpan delay)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            DateTime dateTime = SystemClock.UtcNow.Add(delay);
            _delayedMessages.TryAdd(Guid.NewGuid(), new DelayedMessage
            {
                Message = message,
                MessageType = messageType,
                SendTime = dateTime
            });
            ScheduleNextMaintenance(dateTime);
            return Task.CompletedTask;
        }

        protected override async Task<DateTime?> DoMaintenanceAsync()
        {
            if (this._delayedMessages == null || this._delayedMessages.Count == 0)
            {
                return DateTime.MaxValue;
            }
            DateTime nextMessageSendTime = DateTime.MaxValue;
            List<Guid> list = new List<Guid>();
            DateTime dateTime = SystemClock.UtcNow;
            DateTime t = dateTime.AddMilliseconds(50.0);
            foreach (KeyValuePair<Guid, DelayedMessage> delayedMessage in this._delayedMessages)
            {
                if (delayedMessage.Value.SendTime <= t)
                {
                    list.Add(delayedMessage.Key);
                }
                else if (delayedMessage.Value.SendTime < nextMessageSendTime)
                {
                    nextMessageSendTime = delayedMessage.Value.SendTime;
                }
            }
            bool isTraceLogLevelEnabled = _logger.IsEnabled(0);
            List<Guid>.Enumerator enumerator2 = list.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    Guid current2 = enumerator2.Current;
                    if (this._delayedMessages.TryRemove(current2, out DelayedMessage value))
                    {
                        if (isTraceLogLevelEnabled)
                        {
                            ILogger logger = _logger;
                            object[] obj = new object[2];
                            dateTime = value.SendTime;
                            obj[0] = dateTime.ToString("o");
                            obj[1] = value.MessageType;
                            LoggerExtensions.LogTrace(logger, "Sending delayed message scheduled for {SendTime} for type {MessageType}", obj);
                        }
                        await this.PublishAsync(value.MessageType, value.Message, (TimeSpan?)null, default(CancellationToken)).AnyContext();
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator2).Dispose();
            }
            enumerator2 = default(List<Guid>.Enumerator);
            if (isTraceLogLevelEnabled)
            {
                LoggerExtensions.LogTrace(_logger, "DoMaintenance next message send time: {SendTime}", new object[1]
                {
                nextMessageSendTime.ToString("o")
                });
            }
            return nextMessageSendTime;
        }

        public override void Dispose()
        {
            LoggerExtensions.LogTrace(_logger, "Disposing", Array.Empty<object>());
            _queue.Dispose();
            base.Dispose();
            _delayedMessages?.Clear();
            _subscribers?.Clear();
        }
    }
}
