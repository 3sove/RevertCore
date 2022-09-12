namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class link : CsvSerializationModel<link>
    {
        public string href { get; set; }
        public string rel { get; set; }

        public override string ToCsvString()
        {
            return string.Format("{0},{1}", Escape(href), Escape(rel));
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return string.Format("Link href{0},Link rel{0}", textToAppend);
        }
    }
}
