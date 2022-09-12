using System.IO;

namespace Revert.Core.IO.Serialization
{
    public class ByteArraySerializer : ISerializer<byte[]>
    {
        public byte[] ReadFrom(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }

        public void WriteTo(byte[] value, Stream stream)
        {
            stream.Write(value, 0, value.Length);
        }
    }
}
