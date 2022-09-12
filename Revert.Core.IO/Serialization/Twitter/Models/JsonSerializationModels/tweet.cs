using System.Runtime.Serialization;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    [DataContract]
    public class tweet : CsvSerializationModel<tweet>
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "objectType")]
        public string objectType { get; set; }
        [DataMember(Name = "actor")]
        public actor actor { get; set; }
        [DataMember(Name = "verb")]
        public string verb { get; set; }
        [DataMember(Name = "postedTime")]
        public string postedTime { get; set; } //datetime
        [DataMember(Name = "generator")]
        public generator generator { get; set; }
        [DataMember(Name = "provider")]
        public provider provider { get; set; }
        [DataMember(Name = "link")]
        public string link { get; set; }
        [DataMember(Name = "body")]
        public string body { get; set; }
        [DataMember(Name="object")]
        public twitterObject twitterObject { get; set; }
        [DataMember(Name = "inReplyTo")]
        public inReplyTo inReplyTo { get; set; }
        [DataMember(Name = "location")]
        public location location { get; set; }
        [DataMember(Name = "geo")]
        public tweetLevelGeo geo { get; set; }
        [DataMember(Name = "twitter_entities")]
        public twitterEntities twitter_entities = new twitterEntities();
        [DataMember(Name = "retweetCount")]
        public int retweetCount { get; set; }
        [DataMember(Name = "gnip")]
        public gnip gnip { get; set; }


        /// <summary>
        /// ignores objectType, verb, provider, and object
        /// </summary>
        /// <returns></returns>
        public override string ToCsvString()
        {
            var tweetString = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                Escape(id), actor, postedTime, generator, Escape(link), Escape(body), (inReplyTo ?? new inReplyTo()).ToCsvString(), (location ?? new location()).ToCsvString(), (geo ?? new tweetLevelGeo()).ToCsvString(), 
                (twitter_entities ?? new twitterEntities()).ToCsvString(), retweetCount, (gnip ?? new gnip()).ToCsvString()).Replace("\r\n", " - ").Replace("\n", " - ").Replace("\r", " - ");
            return tweetString;
        }
        //31.21315


        public override string GetCsvSchemaString(string textToAppend)
        {
            return "Tweet Id," + (actor ?? new actor()).GetCsvSchemaString(string.Empty) + ",Tweet posted time," + (generator ?? new generator()).GetCsvSchemaString(string.Empty) +
                ",Tweet link, Tweet body," + (inReplyTo ?? new inReplyTo()).GetCsvSchemaString(string.Empty) + "," + (location ?? new location()).GetCsvSchemaString(string.Empty) +
                "," + (geo ?? new tweetLevelGeo()).GetCsvSchemaString(string.Empty) + "," + (twitter_entities ?? new twitterEntities()).GetCsvSchemaString(string.Empty) + ",Retweet GetCount," +
                (gnip ?? new gnip()).GetCsvSchemaString(string.Empty);
        }


    }
}
