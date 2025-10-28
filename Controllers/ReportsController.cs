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
            for (int i = 0; i < rates.Count; i++)
            {
                var aux = rates[i];

                // Optional: log only the first item
                if (i == 0)
                    break;
            }
            // _logger.LogInformation("**************** " + auxSessions);
            var viewData = sessions.Select(session =>
            {
                var owner = owners.FirstOrDefault(o => o.CarNumber == session.CarNumber);
                var rate = rates.FirstOrDefault(r => r.CityName == session.City && r.ZoneName == session.Zone);

                if (rate != null)
                {
                    _logger.LogInformation($"Rate found: City={rate.CityName}, Zone={rate.ZoneName}, HourlyRate={rate.HourlyRate}");
                }
                else
                {
                    _logger.LogWarning($"No rate found for City={session.City}, Zone={session.Zone}");
                }
                var aux = auxSessions.FirstOrDefault(a =>
                    a.CarNumber == session.CarNumber &&
                    a.StartParkingTime == session.StartParkingTime);
                return new ParkingSessionView
                {
                    OwnerId = owner?.Id ?? "Unknown",
                    OwnerName = owner?.Name ?? "Unknown",
                    CarNumber = session.CarNumber,
                    StartParkingTime = session.StartParkingTime ?? DateTime.MinValue,
                    EndParkingTime = aux?.EndParkingTime ?? DateTime.MinValue, // ✅ Use real end time
                    City = session.City ?? "Unknown",
                    Zone = session.Zone ?? "Unknown",
                    Rate = rate?.HourlyRate ?? 0
                };
            }).ToList();

            return View(viewData);
        }
    }
}
