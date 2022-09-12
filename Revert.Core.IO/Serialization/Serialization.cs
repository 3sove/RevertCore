using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Revert.Core.IO.Serialization
{
    public class Serialization
    {
        public static bool SerializeFile<T>(string path, string fileName, T itemToSerialize)
        {
            string junk;
            return SerializeFile(path, fileName, itemToSerialize, out junk);
        }

        public static bool SerializeFile<T>(string path, string fileName, T itemToSerialize, out string serializedXmlString)
        {
            var pathInfo = new DirectoryInfo(path);
            if (!pathInfo.Exists) pathInfo.Create();

            var filePath = string.Format("{0}\\{1}", path, fileName);
            using (var fs = File.Create(filePath))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var ms = new MemoryStream();
                    new XmlSerializer(typeof(T)).Serialize(ms, itemToSerialize);
                    serializedXmlString = Encoding.UTF8.GetString(ms.ToArray());
                    sw.Write(serializedXmlString);
                    return true;
                }
            }
        }

        public static T DeSerializeFile<T>(string path)
        {
            if (File.Exists(path) == false) throw new FileNotFoundException("No file could be found at the specified path.");
            using (var sr = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                return (T)new XmlSerializer(typeof(T)).Deserialize(new XmlTextReader(new StringReader(sr.ReadToEnd())));
        }
    }
}
