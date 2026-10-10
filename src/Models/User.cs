using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyAppWeb
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public long Id { get; set; }
        public string Unique { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}