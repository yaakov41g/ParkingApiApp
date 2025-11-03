using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ParkingApiApp.ViewModels;

namespace ParkingApiApp.Controllers
{
    [Route("diagrams")] 
    public class DiagramsController : Controller
    {
        private readonly ILogger<DiagramsController> _logger;
        public DiagramsController(ILogger<DiagramsController> logger)
        {
            _logger = logger;

        }
        //[HttpGet]
        [HttpGet("CitySessions")]
        public IActionResult CitySessions()
        {
            var json = HttpContext.Session.GetString("SessionData");
            if (string.IsNullOrEmpty(json))
            {
                return Content("Session data not available.");
            }
            var model = JsonConvert.DeserializeObject<SessionWrapperViewModel>(json);

            // Aggregate sessions by city
            var cityCounts = model.ParkingSessions
                .GroupBy(s => s.City)
                .ToDictionary(g => g.Key, g => g.Count());

            return PartialView("_CitySessions", cityCounts);
        }
        [HttpGet("zone-hours")]
        public IActionResult ZoneHours()
        {
            var json = HttpContext.Session.GetString("SessionData");
            if (string.IsNullOrEmpty(json))
            {
                return Content("Session data not available.");
            }
            var model = JsonConvert.DeserializeObject<SessionWrapperViewModel>(json);

            var grouped = model.ParkingSessions
                .GroupBy(s => $"{s.City} - {s.Zone}")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(s => (s.EndParkingTime - s.StartParkingTime).TotalHours)
                );

            return PartialView("_ZoneHours", grouped);
        }
    }
}
