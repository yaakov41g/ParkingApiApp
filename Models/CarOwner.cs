using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ParkingApiApp.Models
{

    public class CarOwner
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("carNumber")]
        public string CarNumber { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }


        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("bankNumber")]
        public string BankNumber { get; set; }

        [BsonElement("accountNumber")]
        public string AccountNumber { get; set; }
    }
}
