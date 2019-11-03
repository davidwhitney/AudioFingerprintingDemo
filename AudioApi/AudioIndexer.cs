using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Data;

namespace AudioApi
{
    public class AudioIndexer
    {
        private readonly ILogger _logger;
        private readonly IModelService _modelService;
        private readonly IAudioService _audioService;
        private readonly string _tempDir;

        public AudioIndexer(ILogger logger, IModelService modelService, IAudioService audioService)
        {
            _logger = logger;
            _modelService = modelService;
            _audioService = audioService;
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        }

        public async Task Execute(string audioFilePath)
        {
            Directory.CreateDirectory(_tempDir);
            var files = Directory.GetFiles(audioFilePath, "*.mp3", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var (hashedFingerprints, trackInfo) = await ToWaveAndTrack(file);

                _modelService.Insert(trackInfo, hashedFingerprints);
                _logger.LogInformation("Added" + trackInfo);
            }

            Directory.Delete(_tempDir, true);
        }

        private async Task<(List<HashedFingerprint> hashedFingerprints, TrackInfo track)> ToWaveAndTrack(string file)
        {
            await using var mp3 = new Mp3FileReader(file);
            await using var pcm = WaveFormatConversionStream.CreatePcmStream(mp3);

            var filename = Path.GetFileName(file);
            var outputFile = Path.Combine(_tempDir, filename + ".wav");

            WaveFileWriter.CreateWaveFile(outputFile, pcm);

            var hashedFingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(outputFile)
                .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                .UsingServices(_audioService)
                .Hash();

            var tag = TagLib.File.Create(file);
            var track = new TrackInfo(Guid.NewGuid().ToString(), tag.Tag.Title, tag.Tag.Performers.First(), mp3.Length);

            File.Delete(outputFile);

            return (hashedFingerprints, track);
        }
    }
}
