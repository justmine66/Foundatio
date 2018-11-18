using System;
using System.IO;
using System.Text;

namespace Foundatio.Serializer
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer serializer, Stream data)
        {
            return (T)serializer.Deserialize(data, typeof(T));
        }

        public static T Deserialize<T>(this ISerializer serializer, byte[] data)
        {
            return (T)serializer.Deserialize(new MemoryStream(data), typeof(T));
        }

        public static object Deserialize(this ISerializer serializer, byte[] data, Type objectType)
        {
            return serializer.Deserialize(new MemoryStream(data), objectType);
        }

        public static T Deserialize<T>(this ISerializer serializer, string data)
        {
            byte[] buffer = (data == null) ? Array.Empty<byte>() : ((!(serializer is ITextSerializer)) ? Convert.FromBase64String(data) : Encoding.UTF8.GetBytes(data));
            return (T)serializer.Deserialize(new MemoryStream(buffer), typeof(T));
        }

        public static object Deserialize(this ISerializer serializer, string data, Type objectType)
        {
            byte[] buffer = (data == null) ? Array.Empty<byte>() : ((!(serializer is ITextSerializer)) ? Convert.FromBase64String(data) : Encoding.UTF8.GetBytes(data));
            return serializer.Deserialize(new MemoryStream(buffer), objectType);
        }

        public static string SerializeToString<T>(this ISerializer serializer, T value)
        {
            if (value == null)
            {
                return null;
            }
            byte[] array = serializer.SerializeToBytes(value);
            if (serializer is ITextSerializer)
            {
                return Encoding.UTF8.GetString(array);
            }
            return Convert.ToBase64String(array);
        }

        public static byte[] SerializeToBytes<T>(this ISerializer serializer, T value)
        {
            if (value == null)
            {
                return null;
            }
            var memoryStream = new MemoryStream();
            serializer.Serialize(value, memoryStream);
            return memoryStream.ToArray();
        }
    }
}
