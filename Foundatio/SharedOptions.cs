using Foundatio.Serializer;
using Microsoft.Extensions.Logging;

namespace Foundatio
{
    public class SharedOptions
    {
        public ISerializer Serializer { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
