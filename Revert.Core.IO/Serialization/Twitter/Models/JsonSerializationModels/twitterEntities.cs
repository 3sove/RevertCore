using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class twitterEntities : CsvSerializationModel<twitterEntities>
    {
        public List<userMention> user_mentions { get; set; }

        public List<hashtag> hashtags { get; set; }

        public override string ToCsvString()
        {
            var entitiesString = string.Format("{0},{1}", FlattenCollectionToCSV<userMention>(user_mentions, new userMention().ToCsvString(), 6, false),
                                                  FlattenCollectionToCSV<hashtag>(hashtags, new hashtag().ToCsvString(), 6, false));

            return entitiesString;
        }

        public override string GetCsvSchemaString(string textToAppend)
        {
            userMention schemaUserMention = new userMention();
            hashtag schemaHashTag = new hashtag();
            twitterUrl schemaTwitterUrl = new twitterUrl();

            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                schemaUserMention.GetCsvSchemaString(" 1"),
                schemaUserMention.GetCsvSchemaString(" 2"),
                schemaUserMention.GetCsvSchemaString(" 3"),
                schemaUserMention.GetCsvSchemaString(" 4"),
                schemaUserMention.GetCsvSchemaString(" 5"),
                schemaUserMention.GetCsvSchemaString(" 6"),
                schemaHashTag.GetCsvSchemaString(" 1"),
                schemaHashTag.GetCsvSchemaString(" 2"),
                schemaHashTag.GetCsvSchemaString(" 3"),
                schemaHashTag.GetCsvSchemaString(" 4"),
                schemaHashTag.GetCsvSchemaString(" 5"),
                schemaHashTag.GetCsvSchemaString(" 6"));
        }

    }
}
