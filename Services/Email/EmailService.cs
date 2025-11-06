using ParkingApiApp.ViewModels;
using RazorLight;
using System.Net;
using System.Net.Mail;
using System.Text;

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
            using var client = new SmtpClient("smtp.gmail.com")
            {
                Credentials = new NetworkCredential("yaakov41g@gmail.com", "REMOVED"),
                EnableSsl = true
            };

            var mail = new MailMessage("yaakov41g@gmail.com", to)
            {
                Subject = subject,
                Body = body,               // ← your HTML string
                IsBodyHtml = true,         // ✅ tells email client to render HTML
                BodyEncoding = Encoding.UTF8,      // ✅ supports Hebrew and emojis
                SubjectEncoding = Encoding.UTF8    // ✅ supports Hebrew subject line
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
            //string templatePath = "Views/EmailTemplates/Invoice.cshtml";
            string html = await engine.CompileRenderAsync("Invoice.cshtml", account);
            return html;
        }

    }
}
