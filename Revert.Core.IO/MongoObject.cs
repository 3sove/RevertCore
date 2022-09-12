using MongoDB.Bson;

namespace Revert.Core.IO
{
    public class MongoObject<TObject> : IMongoRecord
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public TObject Object { get; set; }

        public MongoObject(TObject _object)
        {
            Object = _object;
        }
    }
}