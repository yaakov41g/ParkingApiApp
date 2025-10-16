    using Google.Cloud.Translate.V3;
using System.Text;

namespace ParkingApiApp.Services
{

    public class TranslationService
    {
        private readonly TranslationServiceClient _client;
        private readonly ILogger<TranslationService> _logger;    

        public TranslationService(ILogger<TranslationService> logger)
        {
            _client = TranslationServiceClient.Create();
            _logger = logger;
        }

        public async Task<string> TranslateHebrewToEnglishAsync(string hebrewText)
        {
            File.AppendAllText("log.txt", $"Hebrew text: {hebrewText}\n", Encoding.UTF8);
            var request = new TranslateTextRequest
            {
                Contents = { hebrewText },
                SourceLanguageCode = "he",
                TargetLanguageCode = "en",
                Parent = "projects/parkingapp-473913/locations/global"
            };

            var response = await _client.TranslateTextAsync(request);
            //_logger.LogInformation($"$$$$$$$$$$$$$ Translated text: {response.Translations[0].TranslatedText}");  
            File.AppendAllText("log.txt", $"Translated text : {response.Translations[0].TranslatedText}\n", Encoding.UTF8);  
            return response.Translations[0].TranslatedText;
        }
    }
}
