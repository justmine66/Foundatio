using Foundatio.Serializer;
using Microsoft.Extensions.Logging;

namespace Foundatio
{
    public class SharedOptionsBuilder<TOption, TBuilder> : OptionsBuilder<TOption> 
        where TOption : SharedOptions, new() 
        where TBuilder : SharedOptionsBuilder<TOption, TBuilder>
    {
        public TBuilder Serializer(ISerializer serializer)
        {
            Target.Serializer = serializer;
            return (TBuilder)this;
        }

        public TBuilder LoggerFactory(ILoggerFactory loggerFactory)
        {
            Target.LoggerFactory = loggerFactory;
            return (TBuilder)this;
        }
    }
}
