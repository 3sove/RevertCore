using System.Linq;

namespace Revert.Core.IO
{
    public class DirectorySearcher
    {
        public static DirectorySearcherModel Search(DirectorySearcherModel model)
        {
            if (!System.IO.Directory.Exists(model.DirectoryPath)) return model;

            model.DirectoryInfo = new System.IO.DirectoryInfo(model.DirectoryPath);
            var searchOption = model.IncludeSubDirectories ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            model.Files = model.DirectoryInfo.GetFiles(model.FileSearchPattern, searchOption).ToList();
            return model;
        }
    }
}
