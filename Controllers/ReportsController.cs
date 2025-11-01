using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using System.Runtime.Intrinsics.X86;

namespace ParkingApiApp.Controllers
{
    [Route("reports")]
    public class ReportsController : Controller
    {
        private readonly IMongoCollection<ParkingSession> _sessionCollection;
        private readonly IMongoCollection<CarOwner> _ownerCollection;
        private readonly IMongoCollection<CityZoneRate> _rateCollection;
        private readonly IMongoCollection<ParkingSessionAux> _auxCollection;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IMongoClient mongoClient,ILogger<ReportsController> logger)
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            _sessionCollection = db.GetCollection<ParkingSession>("parking_sessions");
            _auxCollection = db.GetCollection<ParkingSessionAux>("parking_sessions_aux");
            _ownerCollection = db.GetCollection<CarOwner>("car_owners");
            _rateCollection = db.GetCollection<CityZoneRate>("city_zone_rates");
            _logger=logger;

        }
        [HttpGet("sessions")]
        public async Task<IActionResult> Sessions()
        {
            var sessions = await _sessionCollection.Find(FilterDefinition<ParkingSession>.Empty).ToListAsync();
            var owners = await _ownerCollection.Find(FilterDefinition<CarOwner>.Empty).ToListAsync();
            var rates = await _rateCollection.Find(FilterDefinition<CityZoneRate>.Empty).ToListAsync();
            var auxSessions = await _auxCollection.Find(FilterDefinition<ParkingSessionAux>.Empty).ToListAsync();

            var sessionViews = sessions.Select(session =>
            {
                var owner = owners.FirstOrDefault(o => o.CarNumber == session.CarNumber);
                var rate = rates.FirstOrDefault(r => r.CityName == session.City && r.ZoneName == session.Zone);
                var aux = auxSessions.FirstOrDefault(a =>
                    a.CarNumber == session.CarNumber &&
                    a.StartParkingTime == session.StartParkingTime);

                return new ParkingSessionViewModel
                {
                    OwnerId = owner?.IdNumber ?? "Unknown",
                    OwnerName = owner?.Name ?? "Unknown",
                    CarNumber = session.CarNumber,
                    StartParkingTime = session.StartParkingTime ?? DateTime.MinValue,
                    EndParkingTime = aux?.EndParkingTime ?? DateTime.MinValue,
                    City = session.City ?? "Unknown",
                    Zone = session.Zone ?? "Unknown",
                    Rate = rate?.HourlyRate ?? 0
                };
            }).ToList();

            var accountSummaries = owners.Select(owner =>
            {
                var ownerSessions = sessionViews.Where(s => s.CarNumber == owner.CarNumber).ToList();
                var totalHours = ownerSessions.Sum(s => (s.EndParkingTime - s.StartParkingTime).TotalHours);
                var totalPay = ownerSessions.Sum(s => s.Sum);
                return new SessionAccountViewModel
                {
                    Id = owner.IdNumber,
                    OwnerName = owner.Name,
                    CarNumber = owner.CarNumber,
                    SessionCount = ownerSessions.Count,
                    TotalHours = totalHours,
                    TotalPay = totalPay,
                    Email = owner.Email
                };
            }).ToList();

            var accountDetails = sessionViews.Select((s, index) => new AccountDetailViewModel
            {
                SerialNumber = index + 1,
                City = s.City,
                Zone = s.Zone,
                StartTime = s.StartParkingTime,
                EndTime = s.EndParkingTime,
                Rate = s.Rate
            }).ToList();

            var model = new SessionWrapperViewModel
            {
                ParkingSessions = sessionViews,
                SessionAccounts = accountSummaries,
                AccountDetails = accountDetails
            };

            return View(model);
        }
    }
}
