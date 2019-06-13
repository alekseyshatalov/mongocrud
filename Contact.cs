using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoCrud
{
    public class Contact
    {
        [BsonElement("_id")]
        public BsonObjectId Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<string> Phones { get; set; }
        public BsonDateTime Created { get; set; }
        public BsonBinaryData Photo { get; set; }
    }
}
