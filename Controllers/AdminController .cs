using Microsoft.AspNetCore.Mvc;
using ParkingApiApp.Services;

namespace ParkingApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CitySeederService _seeder;

        public AdminController(CitySeederService seeder)
        {
            _seeder = seeder;
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetCities([FromQuery] string source = "api")
        {
            var result = await _seeder.ResetAndSeedAsync(source);
            return Ok(new { message = result });
        }
    }
}
