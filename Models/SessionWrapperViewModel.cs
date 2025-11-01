namespace ParkingApiApp.Models
{
    public class SessionWrapperViewModel
    {
        public IEnumerable<ParkingSessionViewModel> ParkingSessions { get; set; }
        public IEnumerable<SessionAccountViewModel> SessionAccounts { get; set; }
        public IEnumerable<AccountDetailViewModel> AccountDetails { get; set; }
    }
}
