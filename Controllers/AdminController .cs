using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
namespace ParkingApiApp.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly CitySeederService _seeder;
        private readonly IMongoCollection<City> _cityCollection;
        private readonly IMongoCollection<CityZoneRate> _rateCollection;
        public AdminController(CitySeederService seeder, IMongoClient mongoClient)
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            _cityCollection = db.GetCollection<City>("cities");
            _rateCollection = db.GetCollection<CityZoneRate>("city_zone_rates");
            _seeder = seeder;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetCities([FromQuery] string source = "api")
        {
            var result = await _seeder.ResetAndSeedAsync(source);
            return Ok(new { message = result });
        }
        [HttpPost("seed-rates")]
        public async Task<IActionResult> SeedRates()
        {
            await Seeder.SeedCityZoneRatesAsync(_cityCollection, _rateCollection);
            TempData["Message"] = "Rates seeded successfully.";
            return RedirectToAction("Index");
        }

    }
}
