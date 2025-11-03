using ParkingApiApp.ViewModels;

namespace ParkingApiApp.Services.Email
{
    public interface IEmailService
    {
        Task SendInvoiceAsync(string to, string subject, string body);
        Task SendInvoicesAsync(List<SessionAccountViewModel> accounts);
    }
}
