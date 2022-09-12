using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revert.Core.Extensions;
using Revert.Core.IO.Files;
using ProtoBuf;

namespace Revert.Core.Text.Extraction
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic), ProtoInclude(99, typeof(DocumentInfo))]
    public class TrainingSetItem
    {
        public virtual List<string> Tags { get; set; }
        public virtual string Source { get; set; }

        private string text = string.Empty;

        public virtual string Text
        {
            get
            {
                if (text == string.Empty) text = GetFileContent(Source);
                return text;
            }
            set
            {
                text = value;
            }
        }

        private string GetFileContent(string fileName)
        {
            var file = new FileInfo(fileName);
            if (!file.Exists) throw new Exception("No file was found at " + fileName);

            if (file.Length > 1024 * 1024 * 100) // 100 MB
            {
                //TODO: handle large files
                string fileSummary = string.Empty;
                return fileSummary;
            }

            var extension = file.Extension.Substring(1).ToLowerInvariant();
            if (Images.ImageFileExtensions.Contains(extension)) return string.Empty; //TODO: EXIF
            switch (extension)
            {
                case "txt":
                    return file.ReadText();
                case "csv":
                    var reader = new CsvReader(fileName, true);
                    var rows = reader.Parse();
                    var builder = new System.Text.StringBuilder();
                    foreach (var row in rows.Select(row => StringExtensions.Combine<string>(row, "\t")))
                        builder.AppendLine(row);
                    return builder.ToString();
                default:
                    throw new Exception("Unable to run TIKA parser on .NET Core.");
                    //var tikaParser = new TikaParser();
                    //return tikaParser.Parse(fileName, TikaParserOutput.Text).Content;
            }
        }
    }
}