namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class gnip : CsvSerializationModel<gnip>
    {
        public int klout_score { get; set; }
        public language language { get; set; }

        /// <summary>
        /// returns klout_score and language
        /// </summary>
        public override string ToCsvString()
        {
            return string.Format("{0}, {1}", klout_score, language);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Gnip klout score, Gnip language";
        }
    }
}
