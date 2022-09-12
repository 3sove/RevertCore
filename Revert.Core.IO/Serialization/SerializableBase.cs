using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Revert.Core.Extensions;

namespace Revert.Core.IO.Serialization
{

    public abstract class SerializableBase<T> where T : SerializableBase<T>, new()
    {
        private static readonly T Instance = new T();
        private XmlSerializer serializer;
        protected virtual XmlSerializer Serializer => serializer ?? (serializer = new XmlSerializer(typeof(T), GetAdditionalTypes().Distinct().ToArray()));

        protected static XmlSerializer StaticSerializer => Instance.Serializer;

        public virtual bool Serialize(string path, string fileName)
        {
            try
            {
                var directory = new DirectoryInfo(path);
                if (!directory.Exists) path.CreateDirectory();
                if (!path.EndsWith("\\")) path = path + "\\";

                return Serialize(path + fileName);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual bool Serialize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Directory == null) throw new DirectoryNotFoundException($"The file path requires a directory path and file, such as C:\\Files\\myFile1.xml.  The value supplied was {filePath}");
            if (!fileInfo.Directory.Exists) fileInfo.Directory.FullName.CreateDirectory();

            using (var sw = new StreamWriter(filePath))
            {
                Serializer.Serialize(sw, this);
                return true;
            }
        }

        public static T DeSerializeStatic(string path)
        {
            try
            {
                if (File.Exists(path) == false) throw new FileNotFoundException("No file could be found at the specified path: " + path);
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
                    return (T)StaticSerializer.Deserialize(fs);
            }
            catch
            {
                return default;
            }
        }

        public virtual T DeSerialize(string path)
        {
            try
            {
                if (File.Exists(path) == false) throw new FileNotFoundException("No file could be found at the specified path: " + path);
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
                    return (T)Serializer.Deserialize(fs);
            }
            catch
            {
                return default;
            }
        }

        protected List<Type> additionalTypes;
        protected virtual List<Type> GetAdditionalTypes()
        {
            return additionalTypes ?? (additionalTypes = new List<Type>());
        }

        internal static IEnumerable<Type> GetInheritingMembers(Type baseType)
        {
            return System.Reflection.Assembly.GetAssembly(baseType).GetTypes().Where(baseType.IsAssignableFrom);
        }


    }
}
