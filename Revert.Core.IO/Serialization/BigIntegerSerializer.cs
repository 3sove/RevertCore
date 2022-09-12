using Revert.Core.IO.Serialization;
using System.IO;
using System.Numerics;

namespace Revert.Core.Indexing
{
    public class BigIntegerSerializer : ISerializer<BigInteger>
    {
        public void WriteTo(BigInteger value, Stream stream)
        {
            var valueBytes = value.ToByteArray();
            stream.Write(valueBytes, 0, valueBytes.Length);
        }

        public BigInteger ReadFrom(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return new BigInteger(bytes);
        }
    }
}
