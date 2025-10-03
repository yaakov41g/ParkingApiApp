// These namespaces provide access to Azure's Speech SDK features
using Microsoft.CognitiveServices.Speech;         // Core speech functionality (e.g., config, synthesizer)
using Microsoft.CognitiveServices.Speech.Audio;   // Audio input/output configuration

namespace ParkingApiApp.Services
{
    // This service class handles converting text into spoken audio using Azure's Text-to-Speech
    public class VoiceGeneratorService
    {
        // Your Azure Speech resource credentials
        private readonly string _subscriptionKey = "YOUR_AZURE_SPEECH_KEY"; // Replace with your actual key
        private readonly string _region = "YOUR_AZURE_REGION";               // Replace with your Azure region (e.g., "westeurope")

        // This method takes a text string and generates a voice file saved to disk
        public async Task<string> GenerateVoiceAsync(string text, string fileName)
        {
            // Create a speech configuration using your Azure credentials
            var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);

            // Set the voice to a Hebrew male neural voice (you can change this to other supported voices)
            config.SpeechSynthesisVoiceName = "he-IL-AvriNeural";

            // Define the output path for the audio file (saved in wwwroot/audio/)
            var outputPath = Path.Combine("wwwroot/audio", $"{fileName}.wav");

            // Configure the audio output to write to a WAV file
            using var audioConfig = AudioConfig.FromWavFileOutput(outputPath);

            // Create a speech synthesizer using the config and audio output
            using var synthesizer = new SpeechSynthesizer(config, audioConfig);

            // Perform the actual speech synthesis asynchronously
            var result = await synthesizer.SpeakTextAsync(text);

            // If synthesis is successful, return the relative path to the audio file
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                return $"/audio/{fileName}.wav";

            // If synthesis fails, throw an exception with the reason
            throw new Exception("Speech synthesis failed: " + result.Reason);
        }
    }
}
