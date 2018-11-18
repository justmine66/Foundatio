using System;
using System.IO;

namespace Foundatio.Serializer
{
    public interface ISerializer
    {
        object Deserialize(Stream data, Type objectType);

        void Serialize(object value, Stream output);
    }
}
