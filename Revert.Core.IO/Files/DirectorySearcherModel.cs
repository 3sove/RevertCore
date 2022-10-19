using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.IO
{
    public class DirectorySearcherModel
    {
        public string DirectoryPath { get; set; }
        public System.IO.DirectoryInfo DirectoryInfo { get; set; }
        public string FileSearchPattern { get; set; }
        public bool IncludeSubDirectories { get; set; }
        public List<string> FileNames { get { return Files.Select(f => f.Name).ToList(); } }
        public List<System.IO.FileInfo> Files { get; set; }
    }
}