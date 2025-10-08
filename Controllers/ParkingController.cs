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
        private readonly TexToSpeechService _tts;
        private readonly IMongoCollection<City> _cityCollection;
        private readonly AudioConversionService _audioConverter;
        private readonly ILogger<ParkingController> _logger;
        //private readonly VoiceGeneratorService _tts;
        private readonly SpeechToTextService _speechService;

        public ParkingController(SpeechToTextService speechService, ILogger<ParkingController> logger, AudioConversionService audioConverter, IMongoCollection<City> cityCollection, TexToSpeechService tts)
        {
            _tts = tts;
            _speechService = speechService;
            _audioConverter = audioConverter;
            _logger = logger;
            _cityCollection = cityCollection;
        }

        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            _logger.LogInformation("################# Welcome endpoint hit.");
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
                var message = $"זיהינו את העיר {CityName}. אם זה נכון, הקש 1.";

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

        //[HttpGet("area-response")]
        //public async Task<IActionResult> ListenArea([FromQuery] string city)
        //{
        //    var cityDoc = await _cityCollection.Find(c => c.Name == city).FirstOrDefaultAsync();
        //    if (cityDoc == null)
        //    {
        //        //var errorPath = await _tts.GenerateHebrewVoiceAsync(
        //        //    "מצטערים, לא הצלחנו למצוא את העיר. אנא נסו שוב.",
        //        //    "error_city"
        //        //);
        //     //   return Ok(new { audio = errorPath });
        //    }

        //    var zones = cityDoc.Zones; // Assuming Zones is a list or string field in the city document
        //    var zoneText = string.Join(", ", zones); // Format zones nicely

        //    var finalPath = await _tts.GenerateVoiceAsync(
        //        $"בחרתם את העיר {city} והאזור{(zones.Count > 1 ? "ים" : "")} {zoneText}. המנוי החודשי שלכם יטופל אוטומטית. תודה רבה.",
        //        "final_response"
        //    );

        //    return Ok(new { audio = finalPath });
        //}
    }
}








////////////////////////////////////////////////////////////////
//This version uses Twilio's built-in speech recognition instead of Azure'sGoogle cloud TTS. Why I didn't use Twilio?
//Because I don't have international phone number, and I can't test it properly.  
//
//using Microsoft.AspNetCore.Mvc;
//using Twilio.TwiML;
//using System;
//using Twilio.TwiML.Voice;

//namespace ParkingVoiceApp.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ParkingController : ControllerBase
//    {
//        [HttpPost]
//        public IActionResult StartCall()
//        {
//            var response = new VoiceResponse();

//            var gather = new Gather(
//                input: new[] { Gather.InputEnum.Speech },
//                action: new Uri("/api/parking/city", UriKind.Relative),
//                method: "POST",
//                timeout: 5
//            );
//            gather.Say("Welcome to Smart Parking. Please say the name of your city after the beep.");

//            response.Append(gather);
//            response.Say("We didn't receive any input. Goodbye.");

//            return Content(response.ToString(), "text/xml");//Here Twillio will prompt the user to say the city name
//        }

//        [HttpPost("city")]
//        public IActionResult HandleCity([FromForm] string SpeechResult)
//        {
//            var response = new VoiceResponse();
//            string? city = SpeechResult?.Trim().ToLower();

//            string? matchedCity = city switch
//            {
//                var c when c.Contains("tel aviv") => "Tel Aviv",
//                var c when c.Contains("jerusalem") => "Jerusalem",
//                var c when c.Contains("haifa") => "Haifa",
//                _ => null
//            };

//            if (matchedCity == null)
//            {
//                response.Say("Sorry, we couldn't recognize the city. Please try again later.");
//                return Content(response.ToString(), "text/xml");
//            }

//            var gather = new Gather(
//                input: new[] { Gather.InputEnum.Speech },
//                action: new Uri($"https://skiagraphical-kathey-precise.ngrok-free.dev/api/parking/area?city={matchedCity}"/*, UriKind.Relative*/),
//                method: "POST",
//                timeout: 5
//            );
//            gather.Say($"You selected {matchedCity}. Now say your area: North, Center, or South.");

//            response.Append(gather);
//            response.Say("We didn't receive any input. Goodbye.");

//            return Content(response.ToString(), "text/xml");
//        }

//        [HttpPost("area")]
//        public IActionResult HandleArea([FromQuery] string city, [FromForm] string SpeechResult)
//        {
//            var response = new VoiceResponse();
//            string? area = SpeechResult?.Trim().ToLower();

//            string? matchedArea = area switch
//            {
//                var a when a.Contains("north") => "North",
//                var a when a.Contains("center") => "Center",
//                var a when a.Contains("south") => "South",
//                _ => null
//            };

//            if (matchedArea == null)
//            {
//                response.Say("Sorry, we couldn't recognize the area. Please try again later.");
//                return Content(response.ToString(), "text/xml");
//            }

//            response.Say($"You selected {city} – {matchedArea}. Your monthly parking subscription will be processed automatically. Thank you.");
//            return Content(response.ToString(), "text/xml");
//        }
//    }
//}
