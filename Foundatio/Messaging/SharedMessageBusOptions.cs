using System;
using System.Collections.Generic;

namespace Foundatio.Messaging
{
    public class SharedMessageBusOptions : SharedOptions
    {
        public string Topic { get; set; } = "messages";
        public int TaskQueueMaxItems { get; set; } = 10000;
        public byte TaskQueueMaxDegreeOfParallelism { get; set; } = 4;
        public Dictionary<string, Type> MessageTypeMappings { get; set; } = new Dictionary<string, Type>();
    }
}
