namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class provider : CsvSerializationModel<provider>
    {
        public string objectType { get; set; }
        public string displayName { get; set; }
        public string link { get; set; }

        /// <summary>
        /// ignores objectType and link
        /// </summary>
        public override string ToCsvString()
        {
            return Escape(displayName);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Provider display name";
        }
    }
}
