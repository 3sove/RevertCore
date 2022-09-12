using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class tweetLevelGeo : CsvSerializationModel<tweetLevelGeo>
    {
        public string type { get; set; }
        public List<float> coordinates { get; set; }

        /// <summary>
        /// ignores type
        /// </summary>
        public override string ToCsvString()
        {
            return coordinates != null && coordinates.Count == 2 ? $"\"{coordinates[0]},{coordinates[1]}\"" : string.Empty;
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Tweet Geo";
        }
    }
}
