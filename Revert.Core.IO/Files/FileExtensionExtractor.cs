using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Common.Types.Tries.FileExtensions;

namespace Revert.Core.IO.Files
{
    public class FileExtensionExtractor
    {
        public static bool FixFilesInDirectory(DirectoryInfo directory, bool recursive, ref List<string> filesUpdated)
        {
            if (recursive)
                foreach (var subDirectory in directory.GetDirectories())
                    FixFilesInDirectory(subDirectory, true, ref filesUpdated);

            foreach (var file in directory.GetFiles().Where(file => string.IsNullOrEmpty(file.Extension)))
            {
                var newFileName = string.Format("{0}.{1}", file.FullName, GetFileExtension(file.FullName));
                file.MoveTo(newFileName);
                filesUpdated.Add(newFileName);
            }
            return true;
        }

        private static Trie<byte, string> mimeTree;
        public static string GetFileExtension(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Could not find the specified file.", filePath);

            if (mimeTree == null)
            {
                mimeTree = new Trie<byte,string>(FileSignatures.FileExtensionBySignature);
            }

            throw new NotImplementedException();
            
            ////var extension = 
            //if (mimeTree.TryEvaluate(filePath);
            //if (extension == "zip") extension = GetCorrectZipExtension(filePath, extension);
            //return extension;
        }

        /// <summary>
        /// Resolves the fact that Office 2007 or newer documents are actually zip files
        /// </summary>
        private static string GetCorrectZipExtension(string filePath, string extension)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                var zipArchive = new ZipArchive(fs, ZipArchiveMode.Read, false);
                foreach (var item in zipArchive.Entries)
                {
                    switch (item.FullName)
                    {
                        case "word/document.xml":
                            extension = "docx";
                            break;
                        case "xl/workbook.xml":
                            extension = "xlsx";
                            break;
                        case "ppt/presentation.xml":
                            extension = "pptx";
                            break;
                    }
                }
            }
            return extension;
        }
    }
}
