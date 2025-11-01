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
        private readonly IMongoCollection<CarOwner> _carOwnerCollection;
        private readonly ILogger<AdminController> _logger;  
        public AdminController(CitySeederService seeder, IMongoClient mongoClient, ILogger<AdminController> logger)
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            _cityCollection = db.GetCollection<City>("cities");
            _rateCollection = db.GetCollection<CityZoneRate>("city_zone_rates");
            _carOwnerCollection = db.GetCollection<CarOwner>("car_owners");
            _seeder = seeder;
            _logger = logger;
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
            _logger.LogInformation("@@@@@@@@@@@ Seeding car owners with emails...");

            await Seeder.SeedCityZoneRatesAsync(_cityCollection, _rateCollection);
            TempData["Message"] = "Rates seeded successfully.";
            return RedirectToAction("Index");
        }
        [HttpPost("seed-car-owners")]
        public async Task<IActionResult> SeedCarOwners()
        {
            _logger.LogInformation("@@@@@@@@@@@ Seeding car owners with emails...");
            await Seeder.SeedCarOwnersAsync(_carOwnerCollection, _logger);
            return Ok("Car owners with emails seeded successfully.");
        }
    }
}
