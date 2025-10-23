using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
using StackExchange.Redis;
using System.Media;
using System.Text;

namespace ParkingApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly TranslationService _translationService;
        private readonly CityService _cityService;
        private readonly TexToSpeechService _tts;
        private readonly IMongoCollection<City> _cityCollection;
        private readonly AudioConversionService _audioConverter;
        private readonly ILogger<ParkingController> _logger;
        private readonly IDatabase _redisDb;
        private readonly SpeechToTextService _speechToTextService;
        private static readonly Dictionary<string, ParkingSessionRequest> _sessionData = new();
        private readonly ParkingSessionService _parkingSessionService;
        private readonly CarOwnerRegistrationService _carOwnerService;
        private const string phoneNumber = "0521234567"; //For testing purposes, replace with getting it Twilio query param
        public ParkingController(SpeechToTextService speechToTextService, ILogger<ParkingController> logger,
            AudioConversionService audioConverter, IMongoCollection<City> cityCollection, ParkingSessionService parkingSessionService,
            TexToSpeechService tts, CityService cityService, TranslationService translationService,
            CarOwnerRegistrationService carOwnerService, IConnectionMultiplexer redis)
        {
            _tts = tts;
            _speechToTextService = speechToTextService;
            _audioConverter = audioConverter;
            _logger = logger;
            _cityCollection = cityCollection;
            _cityService = cityService;
            _translationService = translationService;
            _parkingSessionService = parkingSessionService;
            _carOwnerService = carOwnerService;
            _redisDb = redis.GetDatabase();        }

        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            if (!_sessionData.ContainsKey(phoneNumber))
            {
                _sessionData[phoneNumber] = new ParkingSessionRequest
                {
                    PhoneNumber = phoneNumber
                };
            }
            var audioPath = "/audio/welcome.m4a";
            var nextEndpoint = "/api/parking/listen-to-user";

            // Clear Redis cache for cities
            //var db = redis.GetDatabase();
            //await db.KeyDeleteAsync("cities_full");

            return Ok(new
            {
                audio = audioPath,
                next = nextEndpoint,
                clearedKey = "cities_full"
            });
        }

        [HttpPost("listen-to-user")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ListenToUser([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No audio file received.");

            if (!file.ContentType.StartsWith("audio"))
                return BadRequest("Invalid file type. Expected audio.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            var uploadDir = Path.Combine("C:\\ASP\\ParkingApiApp\\Uploads");
            Directory.CreateDirectory(uploadDir);
            var tempPath = Path.Combine(uploadDir, file.FileName);

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string transcript;
            try
            {
                var decompressed = await _audioConverter.ConvertToUncompressedWavAsync(tempPath, uploadDir);
                transcript = await _speechToTextService.TranscribeHebrewAsync(decompressed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transcription");
                return StatusCode(500, "Error during transcription");
            }

            // ✅ Update city only — no need to reinitialize
            _sessionData[phoneNumber].City = transcript;

            return Ok(new { city = transcript });
        }

        [HttpPost("speak-the-message")]
        public async Task<IActionResult> SpeakMessage([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Text is required.");

            try
            {
                // Use your existing TTS service
                var filePath = await _tts.GenerateHebrewVoiceAsync(message); // returns full path
                // Convert to relative path for client
                var fileName = Path.GetFileName(filePath);
                var relativePath = $"/TTS/{fileName}";
                return Ok(new { audio = relativePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during TTS generation");
                return StatusCode(500, "Error during TTS generation");
            }
        }

        [HttpPost("validate-city")]
        public async Task<IActionResult> ValidateCity([FromBody] string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return NotFound(new
                {
                    message = "לא שמעתי את שם העיר. אנא נסה שוב."
                });
            }

            var city = await _cityService.FindCity(cityName);
            if (city == null)
            {
                return NotFound(new
                {
                    message = "העיר לא נמצאה. אנא נסה שוב או נסה עיר אחרת."
                });
            }

            System.IO.File.AppendAllText("log.txt", $"City name : {city.Name}\n", Encoding.UTF8);

            var translatedZones = new List<string>();

            foreach (var zone in city.Zones)
            {
                var hebrewZone = await _translationService.TranslateEnglishToHebrewAsync(zone);
                translatedZones.Add(hebrewZone);
            }

            var zonePrompts = translatedZones.Select((zone, index) =>
                $"הקש {index + 1} ל־{zone}").ToList();

            var optionsMessage = string.Join(", ", zonePrompts);

            return Ok(new
            {
                city = city.Name,
                message = optionsMessage,
                zones = zonePrompts,
                translatedZones = translatedZones // optional for frontend
            });
        }
        //[HttpPost("select-zone")]
        //public IActionResult SelectZone([FromBody] string zone)
        //{
        //    if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(zone))
        //        return BadRequest("Phone number and zone are required.");

        //    if (_sessionData.TryGetValue(phoneNumber, out var session))
        //    {
        //        session.Zone = zone;
        //        System.IO.File.AppendAllText("log.txt", $"Zone selected for {phoneNumber}: {zone}\n", Encoding.UTF8);
        //        return Ok(new { zone });
        //    }

        //    return NotFound("Session not found for this phone number.");
        //}
        [HttpPost("start-session")]
        public async Task<IActionResult> RegisterStartSession([FromQuery] string selectedZone)
        {
            if (!_sessionData.TryGetValue(phoneNumber, out var session))
                return NotFound("Session not found for this phone number.");

            // ✅ Fetch car owner by phone number
            var owner = await _carOwnerService.GetByPhoneNumberAsync(phoneNumber);
            if (owner == null)
                return NotFound("Car owner not found for this phone number.");
            var carKey = $"car:{owner.CarNumber}";
            var startTime = DateTime.Now;
            await _redisDb.StringSetAsync(carKey, startTime.ToString("o")); // ISO 8601 format

            // ✅ Set car number and start time
            session.CarNumber = owner.CarNumber;
            session.Zone = selectedZone;
            session.StartTime = startTime;
            session.EndTime = null; // Ongoing session

            var success = await _parkingSessionService.RegisterSessionAsync(session);

            if (success)
                return Ok("Parking session started.");

            return BadRequest("Failed to start parking session.");
        }
        [HttpPost("end-session")]
        public async Task<IActionResult> RegisterEndSession()
        {
            string phoneNumber = "0521234567"; // Hardcoded for now

            // ✅ Get car number from phone number
            var owner = await _carOwnerService.GetByPhoneNumberAsync(phoneNumber);
            if (owner == null)
                return NotFound("Car owner not found.");

            var carNumber = owner.CarNumber;
            var carKey = $"car:{carNumber}";

            // ✅ Get start time from Redis
            var startTimeStr = await _redisDb.StringGetAsync(carKey);
            if (!startTimeStr.HasValue)
                return NotFound("No active session found for this car.");

            if (!DateTime.TryParse(startTimeStr, out var startTime))
                return BadRequest("Invalid start time format in Redis.");

            var endTime = DateTime.Now;

            // ✅ Update aux collection
            var success = await _parkingSessionService.RegisterEndTimeAsync(carNumber, startTime, endTime);
            if (!success)
                return NotFound("Matching session not found in aux collection.");

            // ✅ Clean up Redis
            await _redisDb.KeyDeleteAsync(carKey);

            return Ok("Parking session ended successfully.");
        }

    }
}
