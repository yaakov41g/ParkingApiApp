namespace ParkingApiApp.Models
{
    public class AccountDetailViewModel
    {
        public int SerialNumber { get; set; }           // #
        public string City { get; set; }                // City
        public string Zone { get; set; }                // Zone
        public DateTime StartTime { get; set; }         // Start Time
        public DateTime EndTime { get; set; }           // End Time
        public decimal Rate { get; set; }               // Rate
        public double Hours => (EndTime - StartTime).TotalHours; // Hours
        public decimal Sum => (decimal)Hours * Rate;    // Sum
    }
}
