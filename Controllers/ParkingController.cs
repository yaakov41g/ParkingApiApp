using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
//using ParkingApiApp.Data;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
using System.Media;

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
        //private readonly VoiceGeneratorService _tts;
        private readonly SpeechToTextService _speechService;

        public ParkingController(SpeechToTextService speechService, ILogger<ParkingController> logger, 
            AudioConversionService audioConverter, IMongoCollection<City> cityCollection, 
            TexToSpeechService tts, CityService cityService, TranslationService translationService)
        {
            _tts = tts;
            _speechService = speechService;
            _audioConverter = audioConverter;
            _logger = logger;
            _cityCollection = cityCollection;
            _cityService = cityService;
            _translationService = translationService;
        }

        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            //_logger.LogInformation("################# Welcome endpoint hit.");
            var audioPath = "/audio/welcome.m4a";
            var nextEndpoint = "/api/parking/listen-city";
            return Ok(new { audio = audioPath, next = nextEndpoint });
        }

        [HttpPost("listen-city")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ListenCity(IFormFile file)
        {
            var transcript = string.Empty;
            if (file == null || file.Length == 0)
                return BadRequest("No audio file received.");

            if (!file.ContentType.StartsWith("audio"))
                return BadRequest("Invalid file type. Expected audio.");

            // Save the file temporarily
            var uploadDir = Path.Combine("C:\\ASP\\ParkingApiApp\\Uploads");
            Directory.CreateDirectory(uploadDir); // Ensure it exists
            var tempPath = Path.Combine(uploadDir, file.FileName);

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            try
            {
                var Decompressed = await _audioConverter.ConvertToUncompressedWavAsync(tempPath, uploadDir);
                transcript = await _speechService.TranscribeHebrewAsync(Decompressed);
            }
            catch (Exception ex)
            {
                ConsolePager.PageText($"Error during transcription:\n{ex}");
                return StatusCode(500, "Error during transcription");
            }
            return Ok(new { city = transcript });
        }

        [HttpPost("speak-city")]
        public async Task<IActionResult> SpeakCity([FromBody] string CityName)
        {
            if (string.IsNullOrWhiteSpace(CityName))
                return BadRequest("Text is required.");

            try
            {
                var message = $"זִיהִינו את העיר {CityName} .אם זה נכון, הַקֵשׁ אישור אם לא הַקֵשׁ אֱמוֹר שׁוּב.";

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
                return BadRequest("City name is required.");
           // _logger.LogInformation("################# CityName " + cityName);

            try
            {
                var city = await _cityService.FindCity(cityName);
                //_logger.LogInformation("################# _cityService " + _cityService);

                if (city == null)
                    return NotFound("City not found.");
                //_logger.LogInformation("################# City "+ city);
                return Ok(new { city = city.Name.ToString(), zones = city.Zones });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating city");
                return StatusCode(500, "Server error");
            }
        }
    }
}
