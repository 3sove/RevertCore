using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Revert.Core.Graph.MetaData.DataPoints;
using Revert.Core.Graph.Vertices;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Graph.MetaData
{
    public class DocumentFeatures : Features
    {
        [DataMember]
        public byte[] FileHash
        {
            get { return BinaryData.FirstOrDefault(t => t.Key == "File Hash")?.Value; }
            set { BinaryData.Add(new BinaryDataPoint("File Hash", value)); }
        }

        public string FilePath
        {
            get { return TextData.FirstOrDefault(t => t.Key == "File Path")?.Value; }
            set { TextData.Add(new TextDataPoint("File Path", value)); }
        }

        /// <summary>
        /// Returns true if similarity is > 80%
        /// </summary>
        public override float CalculateFeatureSimilarity(IVertex entity1, IVertex entity2, TokenIndex tokenIndex, HashSet<string> stopList)
        {
            if (entity1.Features is DocumentFeatures entity1DocumentFeatures 
                && entity2.Features is DocumentFeatures entity2DocumentFeatures 
                && entity1DocumentFeatures.FileHash.SequenceEqual(entity2DocumentFeatures.FileHash))
            {
                return 1f;
            }

            return base.CalculateFeatureSimilarity(entity1, entity2, tokenIndex, stopList);
        }
    }
}
