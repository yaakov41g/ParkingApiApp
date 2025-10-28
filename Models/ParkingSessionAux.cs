using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ParkingApiApp.Models
{
    public class ParkingSessionAux
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  // ✅ This maps the MongoDB _id field
        public string CarNumber { get; set; }
        public DateTime? StartParkingTime { get; set; }
        public DateTime? EndParkingTime { get; set; }
    }
}
