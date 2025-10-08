    using Google.Cloud.TextToSpeech.V1;
    using System.IO;
    using System.Threading.Tasks;

namespace ParkingApiApp.Services
{


    public class TexToSpeechService
    {
        private readonly TextToSpeechClient _client;

        public TexToSpeechService()
        {
            _client = TextToSpeechClient.Create(); // Auth must be set via GOOGLE_APPLICATION_CREDENTIALS
        }

        public async Task<string> GenerateHebrewVoiceAsync(string text)
        {
            var input = new SynthesisInput
            {
                Text = text
            };

            var voice = new VoiceSelectionParams
            {
                LanguageCode = "he-IL", // Hebrew
                SsmlGender = SsmlVoiceGender.Female,
                Name = "he-IL-Wavenet-A" // Optional: specific voice
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = await _client.SynthesizeSpeechAsync(input, voice, audioConfig);

            var outputDir = Path.Combine("C:\\ASP\\ParkingApiApp\\wwwroot", "TTS");
            Directory.CreateDirectory(outputDir);

            var fileName = $"tts_{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(outputDir, fileName);

            using (var output = File.Create(filePath))
            {
                response.AudioContent.WriteTo(output);
            }

            return filePath; // Return path to audio file
        }
    }

}
