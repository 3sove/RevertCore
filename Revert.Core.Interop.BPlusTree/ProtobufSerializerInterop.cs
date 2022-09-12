using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Revert.Core.Interop.BPlusTree
{
    public class ProtobufSerializerInterop<T> : ISerializer<T>
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
