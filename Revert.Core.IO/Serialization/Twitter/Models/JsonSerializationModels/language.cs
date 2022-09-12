namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class language : CsvSerializationModel<language>
    {
        public string value { get; set; }

        public override string ToCsvString()
        {
            return Escape(value);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Language value";
        }
    }
}
