using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Modules;

namespace Revert.Core.Text.Extraction.Gazetteer
{
    public class GazetteerFileReaderModel //: ModuleModel
    {
        private List<List<string>> keywordsAndSynonyms = new List<List<string>>();
        public List<List<string>> KeywordsAndSynonyms
        {
            get { return keywordsAndSynonyms; }
        }

        public System.IO.FileInfo File { get; set; }
    }

    public class GazetteerFileReader //: FunctionalModule<GazetteerFileReader, GazetteerFileReaderModel>
    {
        public void Execute(GazetteerFileReaderModel model, int linesPerUpdate = 1000)
        {
            using (var fileStream = model.File.OpenText())
            {
                string currentLine;
                var lineNumber = 0;
                var linesPerMessage = linesPerUpdate;
                while ((currentLine = fileStream.ReadLine()) != null)
                {
                    if ((lineNumber++ % linesPerMessage) == 1)
                    {
                        Console.WriteLine(string.Format("Reading gazetteer file {0} from {1} to {2}.",
                            model.File.Name, lineNumber.ToString("#,#"), (lineNumber + linesPerMessage - 1).ToString("#,#")));
                    }

                    if (currentLine.Trim() == string.Empty) continue;
                    model.KeywordsAndSynonyms.Add(currentLine.Split('\t').Where(word => word != string.Empty).ToList());
                }
            }
        }
    }



}
