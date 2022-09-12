using System.Collections.Generic;

namespace Revert.Core.Common.Types.Tries.FileExtensions
{
    public class ExtensionTreeModel
    {
        private Dictionary<byte[], string> fileExtensionBySignature;
        public Dictionary<byte[], string> FileExtensionBySignature
        {
            get { return fileExtensionBySignature ?? (fileExtensionBySignature = new Dictionary<byte[], string>()); }
            set { fileExtensionBySignature = value; }
        }
    }
}
