using System;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    [Serializable]
    public class actorLocation : CsvSerializationModel<location>
    {
        public string objectType { get; set; }
        public string displayName { get; set; }

        /// <summary>
        /// Ignores object type, twitter country code, and link
        /// </summary>
        public override string ToCsvString()
        {
            return Escape(displayName);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Actor Location display name";
        }
    }
}
