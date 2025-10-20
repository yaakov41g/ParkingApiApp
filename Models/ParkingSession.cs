using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ParkingApiApp.Models
{
    public class ParkingSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("carNumber")]
        public string CarNumber { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("zone")]
        public string Zone { get; set; }

        [BsonElement("startParkingTime")]
        public DateTime StartParkingTime { get; set; }

        [BsonElement("endParkingTime")]
        public DateTime EndParkingTime { get; set; }
    }
}
