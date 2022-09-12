namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class twitterObject : CsvSerializationModel<twitterObject>
    {
        public string objectType { get; set; }
        public string id { get; set; }
        public string summary { get; set; }
        public string link { get; set; }
        public string postedTime { get; set; } //datetime

        /// <summary>
        /// the twitter object contains no unique fields.  This is not output to the csv
        /// </summary>
        public override string ToCsvString()
        {
            return string.Empty;
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return string.Empty;
        }
    }
}
