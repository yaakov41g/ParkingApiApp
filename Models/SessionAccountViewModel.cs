namespace ParkingApiApp.Models
{
    public class SessionAccountViewModel
    {
        public string SerialNumber { get; set; }
        public string Id { get; set; }                  // Driver ID
        public string OwnerName { get; set; }                // Driver Name
        public string CarNumber { get; set; }           // Car Number
        public int SessionCount { get; set; }           // Sessions Quantity
        public double TotalHours { get; set; }          // Total Hours
        public decimal TotalPay { get; set; }            // Total Pay
        public string Email { get; set; }               // Ema
    }
}
