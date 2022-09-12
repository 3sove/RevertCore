namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class location : CsvSerializationModel<location>
    {
        public string objectType { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string twitter_country_code { get; set; }
        public string link { get; set; }
        public geo geo { get; set; }

        /// <summary>
        /// Ignores object type, twitter country code, and link
        /// </summary>
        public override string ToCsvString()
        {
            var returnValue = string.Format("{0},{1},{2},{3}", Escape(displayName), Escape(name), Escape(country_code), geo ?? new geo());
            return returnValue;
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Location display name,Location name,Location country code," + (geo ?? new geo()).GetCsvSchemaString(string.Empty);
        }
    }
}
