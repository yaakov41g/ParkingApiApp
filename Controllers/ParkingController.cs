using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
using StackExchange.Redis;
using System.Media;
using System.Text;
using ZstdSharp;

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
        private const string phoneNumber = "05311133322"; //For testing purposes, replace with getting it Twilio query param
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
        public async Task<IActionResult> Welcome()
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            var owner = await _carOwnerService.GetByPhoneNumberAsync(phoneNumber);
            if (owner == null)
            {
                // 🔊 Generate TTS message for unregistered user
                var ttsMessage = "אינך רשום עדיין. אנא מָלֵא את פְּרַטֶיךָ כדי להפעיל את החניה.";
                var ttsPath = await _tts.GenerateHebrewVoiceAsync(ttsMessage); // Returns full file path

                // Convert full path to relative URI for frontend
                var relativeUri = ttsPath.Replace("C:\\ASP\\ParkingApiApp\\wwwroot", "").Replace("\\", "/");

                return Ok(new
                {
                    audio = relativeUri, // e.g. "/TTS/tts_abc123.mp3"
                    next = "/signup",
                    isRegistered = false
                });
            }

            // ✅ Registered user: send welcome intro
            if (!_sessionData.ContainsKey(phoneNumber))
            {
                _sessionData[phoneNumber] = new ParkingSessionRequest
                {
                    PhoneNumber = phoneNumber
                };
            }

            var welcomeAudioPath = "/audio/welcome1.m4a";
            var nextEndpoint = "/api/parking/listen-to-user";

            return Ok(new
            {
                audio = welcomeAudioPath,
                next = nextEndpoint,
                isRegistered = true
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
                transcript = await _speechToTextService.TranscribeAsync(decompressed, "he-IL");
                _sessionData[phoneNumber].City = await _speechToTextService.TranscribeAsync(decompressed, "en-US");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transcription");
                return StatusCode(500, "Error during transcription");
            }

            // ✅ Update city only — no need to reinitialize

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
            if (!_sessionData.TryGetValue(phoneNumber, out var session))
                return NotFound("Session not found for this phone number.");
            //_logger.LogInformation($"Validating city: {cityName}");
            session.City = cityName;
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
            session.City = city.Name;

            System.IO.File.AppendAllText("log.txt", $"City name : {city.Name}\n", Encoding.UTF8);

            var translatedZones = new List<string>();

            foreach (var zone in city.Zones)
            {
                var hebrewZone = await _translationService.TranslateEnglishToHebrewAsync(zone);
                translatedZones.Add(hebrewZone);
            }
            var zonePrompts = translatedZones
                .Select((zone, index) => $" הַקֵשׁ {index + 1} ל־{zone}")
                .ToList();
            zonePrompts.Add("לביטול, הַקֵשׁ '0.'");
            var optionsMessage = string.Join(", ", zonePrompts);
            return Ok(new
            {
                city = city.Name,
                message = optionsMessage,
                zones = city.Zones,
                hebrewZones = translatedZones // optional for frontend
            });
        }
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
            //selectedZone = await _speechToTextService.TranscribeAsync("selectedZone", "en-US");
            // ✅ Set car number and start time
            session.CarNumber = owner.CarNumber;
            session.City = session.City;
            session.Zone = selectedZone;
            session.StartTime = startTime;
            session.EndTime = null; // Ongoing session

            var success = await _parkingSessionService.RegisterSessionAsync(session);
            if (success)
                return Ok("Parking session started.");

            return BadRequest("Failed to start parking session.");
        }

        [HttpPost("cancel-session")]
        public async Task<IActionResult> CancelSession()
        {
            // Clear in-memory session
            _sessionData.Remove(phoneNumber);

            // Clear Redis session
            await _redisDb.KeyDeleteAsync($"session:{phoneNumber}");

            return Ok(new { message = "Session cancelled." });
        }

        [HttpPost("end-session")]
        public async Task<IActionResult> RegisterEndSession()
        {
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
