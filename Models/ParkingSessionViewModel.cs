namespace ParkingApiApp.Models
{
    public class ParkingSessionView
    {
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string CarNumber { get; set; }
        public DateTime StartParkingTime { get; set; }
        public DateTime EndParkingTime { get; set; }
        public string City { get; set; }
        public string Zone { get; set; }
        public decimal Rate { get; set; }
        public decimal Sum => (decimal)(EndParkingTime - StartParkingTime).TotalHours * Rate;
    }
}
