using Foundatio.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging
{
    public class InMemoryMessageBus : MessageBusBase<InMemoryMessageBusOptions>
    {
        private readonly ConcurrentDictionary<string, long> _messageCounts = new ConcurrentDictionary<string, long>();

        private long _messagesSent;

        public long MessagesSent => _messagesSent;

        public InMemoryMessageBus()
            : this((InMemoryMessageBusOptionsBuilder o) => o)
        {
        }

        public InMemoryMessageBus(InMemoryMessageBusOptions options)
            : base(options)
        {
        }

        public InMemoryMessageBus(Builder<InMemoryMessageBusOptionsBuilder, InMemoryMessageBusOptions> config)
            : this(config(new InMemoryMessageBusOptionsBuilder()).Build())
        {
        }

        public long GetMessagesSent(Type messageType)
        {
            if (!_messageCounts.TryGetValue(GetMappedMessageType(messageType), out long value))
            {
                return 0L;
            }
            return value;
        }

        public long GetMessagesSent<T>()
        {
            if (!_messageCounts.TryGetValue(base.GetMappedMessageType(typeof(T)), out long value))
            {
                return 0L;
            }
            return value;
        }

        public void ResetMessagesSent()
        {
            Interlocked.Exchange(ref _messagesSent, 0L);
            _messageCounts.Clear();
        }

        protected override Task PublishImplAsync(string messageType, object message, TimeSpan? delay, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _messagesSent);
            _messageCounts.AddOrUpdate(messageType, (string t) => 1L, (string t, long c) => c + 1);
            Type mappedType = GetMappedMessageType(messageType);
            if (_subscribers.IsEmpty)
            {
                return Task.CompletedTask;
            }
            bool flag = _logger.IsEnabled(0);
            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                if (flag)
                {
                    LoggerExtensions.LogTrace(_logger, "Schedule delayed message: {MessageType} ({Delay}ms)", new object[2]
                    {
                    messageType,
                    delay.Value.TotalMilliseconds
                    });
                }
                return AddDelayedMessageAsync(mappedType, message, delay.Value);
            }
            List<Subscriber> list = (from s in _subscribers.Values
                                     where s.IsAssignableFrom(mappedType)
                                     select s).ToList();
            if (list.Count == 0)
            {
                if (flag)
                {
                    LoggerExtensions.LogTrace(_logger, "Done sending message to 0 subscribers for message type {MessageType}.", new object[1]
                    {
                    mappedType.Name
                    });
                }
                return Task.CompletedTask;
            }
            if (flag)
            {
                LoggerExtensions.LogTrace(_logger, "Message Publish: {MessageType}", new object[1]
                {
                mappedType.FullName
                });
            }
            SendMessageToSubscribers(list, mappedType, message.DeepClone());
            return Task.CompletedTask;
        }
    }
}
