using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Revert.Core.IO.Serialization
{
    public interface ISerializer<T>
    {
        T ReadFrom(Stream stream);
        void WriteTo(T value, Stream stream);
    }
}
