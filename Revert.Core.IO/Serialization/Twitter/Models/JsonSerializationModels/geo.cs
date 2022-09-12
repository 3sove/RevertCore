using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class geo : CsvSerializationModel<geo>
    {
        public string type { get; set; }
        public List<List<List<float>>> coordinates { get; set; }

        /// <summary>
        /// format for this return is going to be "type, x1, y1, x2, y2, x3, y3, AdditionalCoordinates in parentheses and quotes "(x4, y4, x5, y5, etc)""
        /// </summary>
        public override string ToCsvString()
        {
            List<string> flattenedCoodinates = new List<string>();
            if (coordinates != null && coordinates[0] != null && coordinates[0][0] != null)
                foreach (var list in coordinates)
                    flattenedCoodinates.AddRange(coordinates.Select(innerList => $"\"{innerList[0]},{innerList[1]}\""));

            return FlattenCollectionToCSV(flattenedCoodinates, string.Empty, 2, false);
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Geo 1,Geo 2";
        }
    }
}
