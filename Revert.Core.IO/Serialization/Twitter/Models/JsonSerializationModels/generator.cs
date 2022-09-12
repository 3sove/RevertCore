namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class generator : CsvSerializationModel<generator>
    {
        public string displayName { get; set; }
        public string link { get; set; }

        /// <summary>
        /// ignores link
        /// </summary>
        public override string ToCsvString()
        {
            return Escape(displayName);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Generator display name";
        }
    }
}
