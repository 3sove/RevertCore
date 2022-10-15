using FASTER.core;
using System.IO;

namespace Revert.Core.IO.Stores
{
    public class FasterProtobufSerializer<T> : IObjectSerializer<T>
    {
        private Stream deserizalizationStream;
        private Stream serializationStream;

        public void BeginDeserialize(Stream stream)
        {
            deserizalizationStream = stream;
        }

        public void Deserialize(out T obj)
        {
            obj = ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(deserizalizationStream, ProtoBuf.PrefixStyle.Base128);
        }

        public void EndDeserialize()
        {
            deserizalizationStream.Close();
            deserizalizationStream.Dispose();
        }

        /// <summary>
        /// Begin serialize
        /// </summary>
        /// <param name="stream"></param>
        public void BeginSerialize(Stream stream)
        {
            serializationStream = stream;
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="obj"></param>
        public void Serialize(ref T obj)
        {
            ProtoBuf.Serializer.SerializeWithLengthPrefix(serializationStream, obj, ProtoBuf.PrefixStyle.Base128);
        }

        public void EndSerialize()
        {
            serializationStream.Close();
            serializationStream.Dispose();
        }
    }
}
