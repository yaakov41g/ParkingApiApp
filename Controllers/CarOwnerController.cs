using Microsoft.AspNetCore.Mvc;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using Microsoft.Extensions.Logging;


namespace ParkingApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarOwnerController : ControllerBase
    {
        private readonly CarOwnerRegistrationService _service;
        private readonly ILogger<CarOwnerController> _logger;

        public CarOwnerController(CarOwnerRegistrationService service, ILogger<CarOwnerController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CarOwner owner)
        {
            _logger.LogInformation("Owner details received for registration: {@Owner}", owner);
            await _service.RegisterAsync(owner);
            return Ok(new { message = "Registration successful" });
        }
    }
}
