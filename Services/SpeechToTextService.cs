using Google.Cloud.Speech.V1;

public class SpeechToTextService
{
    public async Task<string> TranscribeHebrewAsync(string audioFilePath)
    {
        var speech = SpeechClient.Create();
        var response = await speech.RecognizeAsync(new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = 16000,
            LanguageCode = "he-IL"
        }, RecognitionAudio.FromFile(audioFilePath));

        return response.Results
            .SelectMany(r => r.Alternatives)
            .Select(a => a.Transcript)
            .FirstOrDefault() ?? "No transcription found.";
    }
}
