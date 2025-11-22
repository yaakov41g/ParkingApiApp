// Handle HTTP requests for generating parking session reports of tables and diagrams.
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
//using ParkingApiApp.Models;
//using ParkingApiApp.ViewModels;
using ParkingApiApp.Services;
//using System.Runtime.Intrinsics.X86;
using Newtonsoft.Json;

namespace ParkingApiApp.Controllers
{
    [Route("reports")]
    public class ReportsController : Controller
    {
        //private readonly IMongoCollection<ParkingSession> _sessionCollection;
        //private readonly IMongoCollection<CarOwner> _ownerCollection;
        //private readonly IMongoCollection<CityZoneRate> _rateCollection;
        //private readonly IMongoCollection<ParkingSessionAux> _auxCollection;
        private readonly ILogger<ReportsController> _logger;
        private readonly ReportService _reportService;  
        public ReportsController(ReportService reportService, IMongoClient mongoClient,ILogger<ReportsController> logger)
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            //_sessionCollection = db.GetCollection<ParkingSession>("parking_sessions");
            //_auxCollection = db.GetCollection<ParkingSessionAux>("parking_sessions_aux");
            //_ownerCollection = db.GetCollection<CarOwner>("car_owners");
            //_rateCollection = db.GetCollection<CityZoneRate>("city_zone_rates");
            _logger=logger;
            _reportService = reportService; 
        }
        // Load the session report view
        [HttpGet("sessions")]
        public async Task<IActionResult> Sessions()
        {
            var model = await _reportService.BuildSessionReportAsync();
            HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(model));
            return View(model);
        }
    }
}
