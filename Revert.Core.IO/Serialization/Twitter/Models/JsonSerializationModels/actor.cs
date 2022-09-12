using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter.Models.JsonSerializationModels
{
    public class actor : CsvSerializationModel<actor>
    {
        public string objectType { get; set; }
        public string id { get; set; }
        public string link { get; set; }
        public string displayName { get; set; }
        public string postedTime { get; set; } //datetime
        public string image { get; set; }
        public string summary { get; set; }
        public List<link> links { get; set; }
        public int friendsCount { get; set; }
        public int followersCount { get; set; }
        public int listedCount { get; set; }
        public int statusesCount { get; set; }
        public string twitterTimeZone { get; set; }
        public bool verified { get; set; }
        public string utcOffset { get; set; }
        public string preferredUsername { get; set; }
        public List<string> languages { get; set; }
        public actorLocation location { get; set; }
                

        /// <summary>
        /// ignores objectType, link, image, verified
        /// </summary>
        /// <returns></returns>
        public override string ToCsvString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", Escape(id), Escape(displayName), Escape(postedTime), Escape(summary),
                FlattenCollectionToCSV((links ?? new List<link>()), new link().ToCsvString(), 1), 
                friendsCount, followersCount, listedCount, statusesCount, Escape(twitterTimeZone),
                Escape(utcOffset), Escape(preferredUsername), FlattenCollectionToCSV<string>(languages, string.Empty, 1), (location ?? new actorLocation()).ToCsvString());
        }
        
        public override string GetCsvSchemaString(string textToAppend)
        {
            return "User Id,User display name,User account creation date,User summary," +
                new link().GetCsvSchemaString(string.Empty) + ",Additional Links," +  
                "User friends count,User followers count,User listed count,User statuses count,User Twitter timezone,User UTC offset," + 
                "User preferred username,User language,User extra languages," + (location ?? new actorLocation()).GetCsvSchemaString(string.Empty);
        }
    }
}
