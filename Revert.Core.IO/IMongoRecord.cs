using MongoDB.Bson;

namespace Revert.Core.IO
{
    public class MongoRecord : IMongoRecord
    {
        public ObjectId Id { get; set; }
    }

    public interface IMongoRecord
    {
        ObjectId Id { get; set; }
    }
}