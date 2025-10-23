using MongoDB.Bson;

namespace ParkingApiApp.Models
{
    public class ParkingSessionAux
    {
        public string CarNumber { get; set; }
        public DateTime? StartParkingTime { get; set; }
        public DateTime? EndParkingTime { get; set; }
    }
}
