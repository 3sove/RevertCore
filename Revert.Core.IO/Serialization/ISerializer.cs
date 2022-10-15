using System.IO;

namespace Revert.Core.IO.Serialization
{
    public interface ISerializer<T>
    {
        T ReadFrom(Stream stream);
        void WriteTo(T value, Stream stream);
    }
}
