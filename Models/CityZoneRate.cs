using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ParkingApiApp.Models
{
    public class CityZoneRate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // ← Add this line
        public string CityName { get; set; } = "";
        public string ZoneName { get; set; } = "";
        public decimal HourlyRate { get; set; }

    }
}
