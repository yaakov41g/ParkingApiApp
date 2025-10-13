using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ParkingApiApp.Models
{
    public class City
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; } = "";
        //public string Region { get; set; } = "";
        public List<string> Zones { get; set; } = new();
    }
}
