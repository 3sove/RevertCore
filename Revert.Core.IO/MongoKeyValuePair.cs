using MongoDB.Bson;
using Revert.Core.Common.Types;

namespace Revert.Core.IO
{
    public class MongoKeyPair<TOne, TTwo> : KeyPair<TOne, TTwo>, IKeyPair<TOne, TTwo>, IMongoRecord
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public MongoKeyPair(TOne keyOne, TTwo keyTwo)
        {
            this.KeyOne = keyOne;
            this.KeyTwo = keyTwo;
        }

    }
}
