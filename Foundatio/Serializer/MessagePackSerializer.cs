using MessagePack;
using MessagePack.Resolvers;
using System;
using System.IO;
using static MessagePack.LZ4MessagePackSerializer;

namespace Foundatio.Serializer
{
    public class MessagePackSerializer : ISerializer
    {
        private readonly IFormatterResolver _formatterResolver;

        private readonly bool _useCompression;

        public MessagePackSerializer(IFormatterResolver resolver = null, bool useCompression = false)
        {
            //IL_0013: Unknown result type (might be due to invalid IL or missing references)
            _useCompression = useCompression;
            _formatterResolver = resolver ?? ContractlessStandardResolver.Instance;
        }

        public void Serialize(object data, Stream output)
        {
            if (_useCompression)
            {
                NonGeneric.Serialize(data.GetType(), output, data, _formatterResolver);
            }
            else
            {
                NonGeneric.Serialize(data.GetType(), output, data, _formatterResolver);
            }
        }

        public object Deserialize(Stream input, Type objectType)
        {
            if (_useCompression)
            {
                return NonGeneric.Deserialize(objectType, input, _formatterResolver);
            }
            return NonGeneric.Deserialize(objectType, input, _formatterResolver);
        }
    }
}
