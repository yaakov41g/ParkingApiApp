using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApiApp.ViewModels;
using System.Net;
using System.Net.Mail;

namespace ParkingApiApp.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendInvoiceAsync(string to, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:Username"];
            var appPassword = _config["EmailSettings:AppPassword"];

            using var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, to, subject, body);
            await client.SendMailAsync(mail);
            _logger.LogInformation($"Invoice sent to {to}");
        }

        public async Task SendInvoicesAsync(List<SessionAccountViewModel> accounts)
        {
            foreach (var account in accounts)
            {
                var subject = "Monthly Parking Invoice";
                var body = $@"
שלום {account.OwnerName},

הנה החשבונית שלך לחודש האחרון:
• מספר רכב: {account.CarNumber}
• שעות חניה: {account.TotalHours:F2}
• תשלום כולל: ₪{account.TotalPay:F2}

תודה על השימוש בשירותי החניה שלנו!
";

                await SendInvoiceAsync(account.Email, subject, body);
            }
        }
    }
}
