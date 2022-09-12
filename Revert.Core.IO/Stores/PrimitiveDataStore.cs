using Revert.Core.Common.Types.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace Revert.Core.IO.Stores
{
    public class PrimitiveDataStore
    {
        public string Path { get; }

        /// <param name="path">Full path, including filename and extension for your datastore.</param>
        public PrimitiveDataStore(string path)
        {
            Path = path;
        }

        public void put(string key, bool value)
        {
            put(key, "booleans.xml", value);
        }

        public void put(string key, long value)
        {
            put(key, "longs.xml", value);
        }

        public void put(string key, int value)
        {
            put(key, "integers.xml", value);
        }

        public void put(string key, string value)
        {
            put(key, "strings.xml", value);
        }

        public void put(string key, float value)
        {
            put(key, "floats.xml", value);
        }

        public void put(string key, double value)
        {
            put(key, "doubles.xml", value);
        }

        public void put(string key, DateTime value)
        {
            put(key, "dateTimes.xml", value);
        }

        private void put<T>(string key, string storeName, T value)
        {
            using (FileStream zipToOpen = new FileStream(Path, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    var entry = archive.GetEntry(storeName);

                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, T>));

                    SerializableDictionary<string, T> dictionary = null;

                    using (var stream = entry.Open())
                    {
                        dictionary = (SerializableDictionary<string, T>)serializer.Deserialize(stream);
                        dictionary[key] = value;
                        stream.Position = 0;
                        serializer.Serialize(stream, dictionary);
                    }
                }
            }
        }

        public bool getBoolean(string key)
        {
            return get<bool>(key, "booleans.xml");
        }

        public long getLong(string key)
        {
            return get<long>(key, "longs.xml");
        }

        public long getInt(string key)
        {
            return get<int>(key, "integers.xml");
        }

        public string getString(string key)
        {
            return get<string>(key, "strings.xml");
        }

        public float getFloat(string key)
        {
            return get<float>(key, "floats.xml");
        }

        public double getDouble(string key)
        {
            return get<double>(key, "doubles.xml");
        }

        public DateTime getDateTime(string key)
        {
            return get<DateTime>(key, "dateTimes.xml");
        }

        private T get<T>(string key, string storeName)
        {
            using (FileStream zipToOpen = new FileStream(Path, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry(storeName);
                    if (entry == null) return default;

                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, T>));

                    SerializableDictionary<string, T> dictionary = null;

                    using (var stream = entry.Open())
                    {
                        dictionary = (SerializableDictionary<string, T>)serializer.Deserialize(stream);
                        stream.Position = 0;
                        T value;
                        dictionary.TryGetValue(key, out value);
                        return value;
                    }
                }
            }
        }

        private bool tryGet<T>(string key, string storeName, out T returnValue)
        {
            using (FileStream zipToOpen = new FileStream(Path, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry(storeName);
                    if (entry == null)
                    {
                        returnValue = default;
                        return false;
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, T>));

                    SerializableDictionary<string, T> dictionary = null;

                    using (var stream = entry.Open())
                    {
                        dictionary = (SerializableDictionary<string, T>)serializer.Deserialize(stream);
                        stream.Position = 0;
                        return dictionary.TryGetValue(key, out returnValue);
                    }
                }
            }
        }
    }
}
