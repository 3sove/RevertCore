using System.IO;

namespace Revert.Core.IO.Serialization
{
    public class ProtobufSerializer<T> : ISerializer<T>
    {
        public T ReadFrom(Stream stream)
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(stream, ProtoBuf.PrefixStyle.Base128);
        }
        public void WriteTo(T value, Stream stream)
        {
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, value, ProtoBuf.PrefixStyle.Base128);
        }
    }
}
