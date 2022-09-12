using System;
using System.Collections.Generic;
using Revert.Core.IO;

namespace Revert.Core.MachineLearning.DocumentClassification
{
    public class DocumentClassificationResult
    {
        public ITextDocument Document { get; set; }
        public List<Tuple<string, float>> CategoryWeights { get; set; }
        public Dictionary<string, Dictionary<string, float>> CategoryTokenWeights { get; set; }
    }
}
