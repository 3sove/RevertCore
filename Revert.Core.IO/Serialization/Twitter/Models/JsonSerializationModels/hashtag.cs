using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class hashtag : CsvSerializationModel<hashtag>
    {
        public string text { get; set; }
        public List<int> indices { get; set; }

        /// <summary>
        /// ignores indices
        /// </summary>
        public override string ToCsvString()
        {
            return Escape(text);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Hashtag text";
        }
    }
}
