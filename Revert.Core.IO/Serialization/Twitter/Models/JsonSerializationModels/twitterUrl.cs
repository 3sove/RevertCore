using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class twitterUrl : CsvSerializationModel<twitterUrl>
    {
        public string expanded_url { get; set; }
        public List<int> indices { get; set; }
        public string display_url { get; set; }
        public string url { get; set; }

        /// <summary>
        /// Ignores indices, display_url, and url and returns the expanded_url
        /// </summary>
        public override string ToCsvString()
        {
            return Escape(expanded_url);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Expanded URL";
        }
    }
}
