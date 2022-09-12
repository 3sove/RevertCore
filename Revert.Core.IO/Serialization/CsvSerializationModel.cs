using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.IO.Serialization
{
    [Serializable]
    public abstract class CsvSerializationModel<TDerived> where TDerived : CsvSerializationModel<TDerived>
    {
        public abstract string ToCsvString();

        public override string ToString()
        {
            return ToCsvString();
        }

        public abstract string GetCsvSchemaString(string textToAppend);

        public string FlattenCollectionToCSV<T>(IEnumerable<T> models, string emptyModelTemplate, int groupingCountBeforeCombining, bool includeAdditionalItems = true)
        {
            var specifiedModels = string.Empty;
            var modelsArray = models.ToArray();

            var i = 0;
            T model;
            while (i < groupingCountBeforeCombining && i < modelsArray.Length)
            {
                model = modelsArray[i++];
                if (specifiedModels != string.Empty) specifiedModels += ",";
                specifiedModels += model.ToString();
            }

            while (i < groupingCountBeforeCombining)
            {
                if (i > 0) specifiedModels += ",";
                specifiedModels += emptyModelTemplate;
                i++;
            }

            if (!includeAdditionalItems) return specifiedModels;

            var additionalModels = string.Empty;

            while (i < modelsArray.Length)
            {
                model = modelsArray[i++];
                if (additionalModels != string.Empty) additionalModels += " | ";
                additionalModels += Escape(model.ToString());
            }
            return string.Format("{0},{1}", specifiedModels, additionalModels == string.Empty ? string.Empty : string.Format("({0})", additionalModels));
        }

        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

        public static string Escape(string s)
        {
            if (s == null) return string.Empty;
            if (s.Contains(Quote))
                s = s.Replace(Quote, EscapedQuote);

            //s = s.Replace(", ", " - ");
            //s = s.Replace(" ,", " - ");
            //s = s.Replace(",", " - ");

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = Quote + s + Quote;

            return s;
        }
    }
}
