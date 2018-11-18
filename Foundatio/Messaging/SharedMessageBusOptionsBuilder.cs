using System;
using System.Collections.Generic;

namespace Foundatio.Messaging
{
    public class SharedMessageBusOptionsBuilder<TOptions, TBuilder> : SharedOptionsBuilder<TOptions, TBuilder>
        where TOptions : SharedMessageBusOptions, new()
        where TBuilder : SharedMessageBusOptionsBuilder<TOptions, TBuilder>
    {
        public TBuilder Topic(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }

            Target.Topic = topic;
            return (TBuilder)this;
        }
        public TBuilder TaskQueueMaxItems(int maxItems)
        {
            Target.TaskQueueMaxItems = maxItems;
            return (TBuilder)this;

        }
        public TBuilder TaskQueueMaxDegreeOfParallelism(byte maxDegree)
        {
            Target.TaskQueueMaxDegreeOfParallelism = maxDegree;
            return (TBuilder)this;
        }
        public TBuilder MapMessageType<T>(string name)
        {
            if (Target.MessageTypeMappings == null)
            {
                Target.MessageTypeMappings = new Dictionary<string, Type>();
            }
            Target.MessageTypeMappings[name] = typeof(T);
            return (TBuilder)this;
        }
        public TBuilder MapMessageTypeToClassName<T>()
        {
            if (Target.MessageTypeMappings == null)
            {
                Target.MessageTypeMappings = new Dictionary<string, Type>();
            }
            Target.MessageTypeMappings[typeof(T).Name] = typeof(T);
            return (TBuilder)this;
        }
    }
}
