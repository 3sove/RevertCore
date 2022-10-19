using System;
using System.Collections.Generic;
using Revert.Core.IO;

namespace Revert.Core.Text.Extraction.Gazetteer
{
    public class GazetteerGeneratorModel
    {
        public string DirectoryPath { get; set; }
        public bool IncludeSubDirectories { get; set; }
        public string FileSearchPattern { get; set; }
        public GazetteerGeneratorModel()
        {
            FileSearchPattern = "*.lst";
        }
        
        public HashSet<string> GetKeywords()
        {
            var keywords = new HashSet<string>();
            foreach (var termAndSynonymList in KeywordsAndSynonymsByFileName.Values)
                foreach (var synonymList in termAndSynonymList)
                    foreach (var synonym in synonymList)
                        keywords.Add(synonym.ToUpper());
            return keywords;
        }

        public Dictionary<string, List<string>> FileNamesByKeyword
        {
            get
            {
                var fileNamesByKeyword = new Dictionary<string, List<string>>();

                foreach (var kvp in KeywordsAndSynonymsByFileName)
                    foreach (var keywordAndSynonymList in kvp.Value)
                        foreach (var synonym in keywordAndSynonymList)
                        {
                            List<string> fileNames;
                            if (!fileNamesByKeyword.TryGetValue(synonym, out fileNames) || fileNames == null)
                            {
                                fileNames = new List<string>();
                                fileNamesByKeyword[synonym] = fileNames;
                            }
                            fileNames.Add(synonym);
                        }
                return fileNamesByKeyword;
            }
        }

        public Dictionary<string, List<List<string>>> KeywordsAndSynonymsByFileName { get; set; } = new Dictionary<string, List<List<string>>>();
    }

    public class GazetteerGenerator
    {
        public void Execute(GazetteerGeneratorModel model)
        {
            var directorySearchModel = DirectorySearcher.Search(
                new DirectorySearcherModel
                {
                    DirectoryPath = model.DirectoryPath,
                    FileSearchPattern = model.FileSearchPattern,
                    IncludeSubDirectories = model.IncludeSubDirectories
                });

            foreach (var file in directorySearchModel.Files)
            {
                Console.WriteLine($"Evaluating {file.Name}.");
                var gazetteeerFileReader = new GazetteerFileReader();
                var fileReaderModel = new GazetteerFileReaderModel { File = file };
                gazetteeerFileReader.Execute(fileReaderModel);
                model.KeywordsAndSynonymsByFileName.Add(fileReaderModel.File.Name, fileReaderModel.KeywordsAndSynonyms);
            }
        }
    }
}
