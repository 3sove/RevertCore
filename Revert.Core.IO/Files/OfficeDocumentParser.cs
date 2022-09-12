using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Revert.Core.Extensions;

namespace Revert.Core.IO.Files
{
    public class OfficeDocumentParser
    {
        private static readonly HashSet<string> officeFileTypes = new HashSet<string>(new[]
        {
            ".DOCX", ".PPTX", ".XLSX"
        });

        public static bool IsOfficeDocument(FileInfo file)
        {
            return officeFileTypes.Contains(file.Extension);
        }

        private static readonly Dictionary<string, HashSet<string>> fileHashesByDirectoryPath = new Dictionary<string, HashSet<string>>();
        public static OfficeDocumentResult EvaluateOfficeDocument(FileInfo info, DirectoryInfo targetDirecory)
        {
            var result = new OfficeDocumentResult();
            try
            {
                string directory = info.DirectoryName ?? string.Empty;
                using (var fs = info.OpenRead())
                {
                    var zipArchive = new ZipArchive(fs, ZipArchiveMode.Read, false);
                    foreach (var item in zipArchive.Entries)
                    {
                        try
                        {
                            if (MimeTypeExtractor.ImageFileTypes.Contains(item.Name.GetFileExtension().ToUpper()))
                            {
                                using (var stream = item.Open())
                                {
                                    var fileHash = Encoding.UTF8.GetString(new SHA1Managed().ComputeHash(stream));

                                    HashSet<string> fileHashes;
                                    if (!fileHashesByDirectoryPath.TryGetValue(directory, out fileHashes))
                                        fileHashesByDirectoryPath[directory] = fileHashes = new HashSet<string>();

                                    if (!fileHashes.Add(fileHash))
                                        continue; //TODO: Handle duplicate files better
                                }

                                //we have to do this twice because office documents style zips don't support seeking
                                using (var stream = item.Open())
                                {
                                    if (!targetDirecory.Exists) targetDirecory.Create();
                                    var newFileName = targetDirecory.GetNewFileName(
                                        string.Format("{0} - {1}", info.Name.GetFileNameWithoutExtension(), item.Name));
                                    result.EmbeddedFiles.Add(newFileName);

                                    using (var file = File.OpenWrite(newFileName))
                                        stream.CopyTo(file);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(ex.GetBaseException().Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.GetBaseException().Message); //throw the error on the floor and just move on - apparently bad office documents throw many exceptions for stupid reasons
            }
            return result;
        }



    }
}