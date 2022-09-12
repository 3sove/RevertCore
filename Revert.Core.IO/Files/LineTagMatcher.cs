using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.IO.Files
{
    

    public class LineTagMatcher
    {
        private string[] Terms { get; set; }

        private IEnumerable<string> LineReader { get; set; }

        private string OutputDirectory { get; set; }

        public LineTagMatcher(string[] terms, IEnumerable<string> lineReader, string outputDirectory)
        {
            Terms = terms;
            LineReader = lineReader;
            OutputDirectory = outputDirectory;
            if (!OutputDirectory.EndsWith("\\")) OutputDirectory = OutputDirectory + "\\";
        }

        public void Execute()
        {
            var terms = Terms.Select(t => t.ToUpper()).ToArray();

            var lineMatchesByMatchTerm = new Dictionary<string, List<string>>();

            List<string> lineMatches = null;

            var lineReader = LineReader;

            foreach (var line in lineReader)
            {
                if (line == null) break;

                var upperLine = line.ToUpper();
                foreach (var term in terms)
                {
                    if (upperLine.Contains(term))
                    {
                        if (!lineMatchesByMatchTerm.TryGetValue(term, out lineMatches))
                        {
                            lineMatches = new List<string>();
                            lineMatchesByMatchTerm[term] = lineMatches;
                        }
                        lineMatches.Add(line);
                    }
                }
            }
            
            foreach (var matchesByTerm in lineMatchesByMatchTerm)
            {
                using (var fileWriter = System.IO.File.CreateText(OutputDirectory + matchesByTerm.Key))
                {
                    foreach (var line in matchesByTerm.Value) fileWriter.WriteLine(line);
                }
            }

        }

    }
}
