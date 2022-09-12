namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class inReplyTo : CsvSerializationModel<inReplyTo>
    {
        public string link { get; set; }

        public override string ToCsvString()
        {
            return Escape(link ?? string.Empty);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "In reply to link";
        }
    }
}
