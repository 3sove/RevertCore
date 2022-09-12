using System.Collections.Generic;

namespace Revert.Core.IO.Files
{
    public class OfficeDocumentResult
    {
        private List<string> errors;
        private List<string> embeddedFiles;

        public List<string> Errors
        {
            get { return errors ?? (errors = new List<string>()); }
            set { errors = value; }
        }

        public List<string> EmbeddedFiles
        {
            get { return embeddedFiles ?? (embeddedFiles = new List<string>()); }
            set { embeddedFiles = value; }
        }
    }
}