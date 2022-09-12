using System.IO;
using Revert.Core.GuitarProReader.GuitarPro.VersionSpecific;

namespace Revert.Core.GuitarProReader.GuitarPro
{
    public class GuitarProFileFactory
    {
        public static GuitarProFile CreateFile(Stream fileStream)
        {
            var gpStream = new GpInputStream(fileStream);
            var version = gpStream.GetVersionFromString();
            switch (version.Major)
            {
                case "5":
                    return Gp5InputStream.Create(fileStream, version);
                case "4":
                    return Gp4InputStream.Create(fileStream, version);
                case "3":
                    return Gp3InputStream.Create(fileStream, version);
            }
            return null;
        }
    }
}
