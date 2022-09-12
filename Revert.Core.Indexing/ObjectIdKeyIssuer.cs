using System;
using MongoDB.Bson;
using Revert.Core.Common;

namespace Revert.Core.Indexing
{
    public class ObjectIdKeyIssuer : KeyGenerator<ObjectId>
    {
        public static ObjectIdKeyIssuer Instance = new ObjectIdKeyIssuer(ObjectId.GenerateNewId());

        public ObjectIdKeyIssuer(ObjectId startingId) : base(startingId, id => ObjectId.GenerateNewId())
        {
        }
    }
}