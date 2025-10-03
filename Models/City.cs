namespace ParkingApiApp.Models
{
    public class City
    {
        public string Name { get; set; } = "";
        //public string Region { get; set; } = "";
        public List<string> Zones { get; set; } = new();
    }
}
