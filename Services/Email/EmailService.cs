using ParkingApiApp.ViewModels;
using RazorLight;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ParkingApiApp.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(ILogger<EmailService> logger, EmailSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task SendInvoiceAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_settings.SmtpServer)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Username, _settings.AppPassword),
                EnableSsl = true
            };

            var mail = new MailMessage(_settings.Username, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            await client.SendMailAsync(mail);
            _logger.LogInformation($"Invoice sent to {to}");
        }

        public async Task SendInvoicesAsync(List<SessionAccountViewModel> accounts)
        {
            foreach (var account in accounts)
            {
                var subject = "Monthly Parking Invoice";
                var body = await RenderInvoiceHtmlAsync(account);
                await SendInvoiceAsync(account.Email, subject, body);
            }
        }

        private async Task<string> RenderInvoiceHtmlAsync(SessionAccountViewModel account)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Views", "EmailTemplates"))
                .UseMemoryCachingProvider()
                .Build();

            string html = await engine.CompileRenderAsync("Invoice.cshtml", account);
            return html;
        }
    }
}
