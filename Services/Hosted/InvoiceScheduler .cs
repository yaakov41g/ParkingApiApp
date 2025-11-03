using ParkingApiApp.Services.Email;

namespace ParkingApiApp.Services.Hosted
{
    public class InvoiceScheduler : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<InvoiceScheduler> _logger;

        public InvoiceScheduler(IServiceProvider services, ILogger<InvoiceScheduler> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // Run only on the 4th of the month at 08:00
                if (now.Day == 4 && now.Hour == 0 && now.Minute == 58)
                {
                    using var scope = _services.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();

                    var report = await reportService.BuildSessionReportAsync();
                    await emailService.SendInvoicesAsync(report.SessionAccounts.ToList());

                    _logger.LogInformation("Invoices sent at " + now);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
            }
        }
    }
}
