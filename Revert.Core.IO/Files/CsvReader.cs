using System;
using System.Collections.Generic;

namespace Revert.Core.IO.Files
{
    public class CsvReader : IDisposable
    {
        public string FilePath { get; private set; }
        public bool HasHeaders { get; private set; }
        private readonly LumenWorks.Framework.IO.Csv.CachedCsvReader reader;

        public CsvReader(string filePath, bool hasHeaders)
        {
            FilePath = filePath;
            HasHeaders = hasHeaders;
            reader = new LumenWorks.Framework.IO.Csv.CachedCsvReader(new System.IO.StreamReader(FilePath), HasHeaders);
        }

        public string[] GetHeaders()
        {
            return reader.GetFieldHeaders();
        }

        public IEnumerable<string[]> Parse()
        {
            return reader;
        }

        public IEnumerable<string[]> Parse(System.IO.TextReader textReader)
        {            
            return new LumenWorks.Framework.IO.Csv.CachedCsvReader(textReader, HasHeaders);
        }
        
        public void Dispose()
        {
        }
    }
}
