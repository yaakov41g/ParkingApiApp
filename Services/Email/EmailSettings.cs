namespace ParkingApiApp.Services.Email
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string AppPassword { get; set; }
    }
}
