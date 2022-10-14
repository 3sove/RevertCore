using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Revert.Core.Extensions;
using Revert.Core.Graph.MetaData;
using Revert.Core.IO;
using Revert.Core.Graph.MetaData.DataPoints;

namespace Revert.Core.Graph.Vertices
{
    [DataContract]
    [DebuggerDisplay("{GetEntityDebuggerDisplay(),nq}")]
    public class DocumentEntity : Entity
    {
        public DocumentEntity()
        {
        }

        public override string GetEntityDebuggerDisplay()
        {
            return $"{Name} -- {Features.SummarizeDataPoints()}";
        }

        public DocumentEntity(TextDocument document, DocumentFeatures features)
        {
            Name = document.Name;
            Features.EntityType = "Document";
            Features = features;
            if (document.FileHash != null)
                ((DocumentFeatures)Features).FileHash = document.FileHash;
            else if (File.Exists(document.FilePath))
                using (var fileStream = File.OpenRead(document.FilePath))
                    ((DocumentFeatures)Features).FileHash = fileStream.ComputeHash();
        }

        public DocumentEntity(TextDocument document, bool includeDirectoryAsCategory = false, int documentId = 0)
        {
            Name = document.Name;
            Features = new DocumentFeatures
            {
                EntityType = "Document",
                FilePath = document.FilePath ?? string.Empty,
                TextData = new HashSet<TextDataPoint>
                {
                    new TextDataPoint("Name", document.Name) { IsResolvable = true },
                    new TextDataPoint("Text", document.Text) { IsResolvable = false, IsSearchable = true},
                    new TextDataPoint("Type", "Document")
                },
            };

            if (includeDirectoryAsCategory) Features.TextData.Add(new TextDataPoint("Category", document.DirectoryName));

            if (documentId != 0) Features.DiscreteData.Add(new DiscreteDataPoint("Id", documentId));

            if (document.FileHash != null)
                ((DocumentFeatures)Features).FileHash = document.FileHash;
            else if (File.Exists(document.FilePath))
                using (var fileStream = File.OpenRead(document.FilePath))
                    ((DocumentFeatures)Features).FileHash = fileStream.ComputeHash();
        }


    }
}
