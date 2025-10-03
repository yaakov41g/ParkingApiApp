using Microsoft.AspNetCore.Mvc;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
using System.Media;

namespace ParkingApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly ILogger<ParkingController> _logger;
        private readonly VoiceGeneratorService _tts;
        private readonly SpeechToTextService _speechService;
        public ParkingController(VoiceGeneratorService tts, SpeechToTextService speechService, ILogger<ParkingController> logger)
        {
            _tts = tts;
            _speechService = speechService;
            _logger = logger;
        }

        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            var audioPath = "/audio/welcome.m4a";
            _logger.LogInformation($"############### Uploaded welcome file from  : {audioPath}");

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

            //var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                _logger.LogInformation($"Saving uploaded file to ###############: {tempPath}");
            }
            var player = new SoundPlayer(tempPath);
            player.PlaySync(); // blocks until playback finishes

            // Transcribe using Google Speech-to-Text
            try
            {
                transcript = await _speechService.TranscribeHebrewAsync(tempPath);
            }
            catch (Exception ex)
            {
                ConsolePager.PageText($"Error during transcription:\n{ex}");
                //_logger.LogError(ex, "Error during transcription");
                return StatusCode(500, "Error during transcription");
            }
            return Ok(new { city = transcript });
        }

        [HttpGet("area-response")]
        public async Task<IActionResult> AreaResponse([FromQuery] string city, [FromQuery] string area)
        {
            string? matchedArea = area?.Trim().ToLower() switch
            {
                var a when a.Contains("north") => "צפון",
                var a when a.Contains("center") => "מרכז",
                var a when a.Contains("south") => "דרום",
                _ => null
            };

            if (matchedArea == null)
            {
                var errorPath = await _tts.GenerateVoiceAsync(
                    "מצטערים, לא הצלחנו לזהות את האזור. אנא נסו שוב.",
                    "error_area"
                );
                return Ok(new { audio = errorPath });
            }

            var finalPath = await _tts.GenerateVoiceAsync(
                $"בחרתם את העיר {city} והאזור {matchedArea}. המנוי החודשי שלכם יטופל אוטומטית. תודה רבה.",
                "final_response"
            );

            return Ok(new { audio = finalPath });
        }

        [HttpGet("play")]
        public IActionResult PlayAudio()
        {
            var filePath = Path.Combine("C:\\ASP\\ParkingApiApp\\Uploads", "sample.wav");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Audio file not found.");

            try
            {
                //_logger.LogInformation($"Upload file from ###############: {filePath}");

                var player = new SoundPlayer(filePath);
                player.PlaySync(); // blocks until playback finishes
                return Ok("Audio played successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error playing audio: {ex.Message}");
            }
        }
    }
}








////////////////////////////////////////////////////////////////
//This version uses Twilio's built-in speech recognition instead of Azure's TTS
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
