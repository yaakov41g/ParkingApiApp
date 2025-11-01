using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ParkingApiApp.ViewModels;

namespace ParkingApiApp.Controllers
{
    public class DiagramsController : Controller
    {
        private readonly ILogger<DiagramsController> _logger;
        public DiagramsController(ILogger<DiagramsController> logger)
        {
            _logger = logger;

        }
        [HttpGet]
        public IActionResult CitySessions()
        {
            var json = TempData["SessionData"] as string;
            if (string.IsNullOrEmpty(json))
            {
                return Content("Session data not available.");

            }

            var model = JsonConvert.DeserializeObject<SessionWrapperViewModel>(json);

            // Aggregate sessions by city
            var cityCounts = model.ParkingSessions
                .GroupBy(s => s.City)
                .ToDictionary(g => g.Key, g => g.Count());
            _logger.LogInformation("CITYCOUNTS diagram: " + JsonConvert.SerializeObject(cityCounts));

            return PartialView("_CitySessions", cityCounts);
        }
        [HttpGet("zone-hours")]
        public IActionResult ZoneHours(string city, string zone)
        {
            var json = TempData["SessionData"] as string;
            if (string.IsNullOrEmpty(json)) return PartialView("_ZoneHours", new Dictionary<string, double>());

            var model = JsonConvert.DeserializeObject<SessionWrapperViewModel>(json);
            var filteredSessions = model.ParkingSessions
                .Where(s => s.City == city && s.Zone == zone)
                .ToList();

            var grouped = filteredSessions
                .GroupBy(s => s.CarNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(s => (s.EndParkingTime - s.StartParkingTime).TotalHours)
                );

            return PartialView("_ZoneHours", grouped); // Dictionary<string, double>
        }

    }
}
