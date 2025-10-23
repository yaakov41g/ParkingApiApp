namespace ParkingApiApp.Models
{
    public class ParkingSessionRequest
    {
        public string PhoneNumber { get; set; }
        public string CarNumber { get; set; }
        public string City { get; set; }
        public string Zone { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; } // Nullable for active sessions
    }
}
