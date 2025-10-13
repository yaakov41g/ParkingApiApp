using System.Diagnostics;
using System.IO;
using System.Media;
namespace ParkingApiApp.Services
{
    public class AudioConversionService
    {
        private readonly ILogger<AudioConversionService> _logger;
        private readonly string _ffmpegPath;

        public AudioConversionService(ILogger<AudioConversionService> logger)
        {         

            _logger = logger;
            _ffmpegPath = @"C:\Program Files\ffmpeg-8.0-essentials_build\bin\ffmpeg.exe"; // Update this path if needed
        }

        public async Task<string> ConvertToUncompressedWavAsync(string inputPath, string outputDirectory)
        {
            //_logger.LogInformation("################  Checking conversion ");

            var outputFileName = "converted_" + Path.GetFileName(inputPath);
            var outputPath = Path.Combine(outputDirectory, outputFileName);

            var args = $"-y -i \"{inputPath}\" -acodec pcm_s16le -ac 1 -ar 16000 \"{outputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string errorOutput = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"FFmpeg failed: {errorOutput}");
                throw new Exception("Audio conversion failed.");
            }
            //tomorrow test if ffmpeg conversion works
            try
            {
            var player = new SoundPlayer(outputPath);

                player.PlaySync(); // blocks until playback finishes
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error playing audio: {ex.Message}");
            }
            _logger.LogInformation($"FFmpeg conversion successful: {outputPath}");
            return outputPath;
        }
    }
}
