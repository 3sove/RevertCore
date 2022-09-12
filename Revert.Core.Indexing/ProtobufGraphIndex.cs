using System;
using System.Collections.Generic;
using System.IO;
using Revert.Core.Extensions;
using ProtoBuf;

namespace Revert.Core.Indexing
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ProtobufGraphIndex<TValue> : ProtobufGraphIndex<int, TValue> where TValue : IProtobufGraphEntity<int>
    {
        public ProtobufGraphIndex(string filePath)
            : base(filePath)
        {
        }

        public virtual void Add(TValue value)
        {
            value.Id = Index.Count + 1;
            Index[value.Id] = value;
        }
    }

    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public interface IProtobufGraphEntity<TKey>
    {
        TKey Id { get; set; }
    }

    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ProtobufGraphIndex<TKey, TValue> : ProtobufGraphIndex
    {
        public ProtobufGraphIndex(string filePath)
            : base(filePath)
        {
            Index = new Dictionary<TKey, TValue>();
            Load();
        }

        public Dictionary<TKey, TValue> Index { get; set; }

        public virtual void Add(TKey key, TValue value)
        {
            //lock (UIntKeyIndex)
            //{
            //if (UIntKeyIndex.ContainsKey(key)) throw new Exception("An item with that key has already ben added to the graph");
            Index[key] = value;
            //}
        }

        public virtual void Save()
        {
            var ms = new MemoryStream();
            Serializer.Serialize(ms, Index);
            ms.Position = 0;


            var fileInfo = new FileInfo(FilePath);
            if (fileInfo.Directory == null) throw new DirectoryNotFoundException($"The file path requires a directory path and file, such as C:\\Files\\myFile1.xml.  The value supplied was {FilePath}");
            if (!fileInfo.Directory.Exists) fileInfo.Directory.FullName.CreateDirectory();

            using (var fileStream = File.OpenWrite(FilePath))
                fileStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
        }

        public delegate void Load_Completed();

        public event Load_Completed LoadCompleted;

        public virtual void Load()
        {
            if (!File.Exists(FilePath)) return;

            using (var stream = File.OpenRead(FilePath))
                Index = Serializer.Deserialize<Dictionary<TKey, TValue>>(stream);

            Console.WriteLine("Done loading {0}", FilePath);
            LoadCompleted?.Invoke();
        }
    }

    public abstract class ProtobufGraphIndex
    {
        public string FilePath { get; set; }

        protected ProtobufGraphIndex(string filePath)
        {
            FilePath = filePath;
        }
    }
}
