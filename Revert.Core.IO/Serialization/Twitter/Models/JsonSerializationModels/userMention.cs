using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class userMention : CsvSerializationModel<userMention>
    {
        public List<int> indices { get; set; }
        public string screen_name { get; set; }
        public string id_str { get; set; }
        public string name { get; set; }
        //public int? id { get; set; }

        /// <summary>
        /// ignores indices and id, but displays id_string which is simply the id as text
        /// </summary>
        public override string ToCsvString()
        {
            return string.Format("{0},{1},{2}", id_str, Escape(screen_name), Escape(name));
        }

        /// <param name="textToAppend">include spaces if you want there to be spaces</param>
        public override string GetCsvSchemaString(string textToAppend)
        {
            return string.Format("User mention Id{0},User mention screen name{0},User mention name{0}", textToAppend);
        }
    }
}
