using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.RAW;
using Microsoft.AspNetCore.Mvc;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Configuration;

namespace AudioApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AudioSearchController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly IAudioService _audioService;

        public AudioSearchController(IModelService modelService, IAudioService audioService)
        {
            _modelService = modelService;
            _audioService = audioService;
        }

        [HttpPost]
        [Consumes("audio/webm; codecs=pcm")]
        public async Task<ActionResult<BestMatch>> PostWebM()
        {
            await using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var bytes = ms.ToArray();

            await using var ms2 = new MemoryStream(bytes) {Position = 0};
            var ff = new CSCore.WaveFormat(48000, 16, 2, AudioEncoding.IeeeFloat);
            var dr = new RawDataReader(ms2, ff);
            
            await using var outs = new MemoryStream();
            dr.WriteToWaveStream(outs);
            bytes = outs.ToArray();

            return await MatchFingerprintAsWav(bytes);
        }

        [HttpPost]
        [Consumes("audio/webm; codecs=opus")]
        public async Task<ActionResult<BestMatch>> PostOpus()
        {
            await using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var sampleRate = int.Parse(Request.Headers["sample-rate"].FirstOrDefault() ?? "48000");
            var channelCount = int.Parse(Request.Headers["channel-count"].FirstOrDefault() ?? "2");
            bytes = new OpusTranscoder(sampleRate, channelCount).ToWav(bytes);

            return await MatchFingerprintAsWav(bytes);
        }

        [HttpPost]
        [Consumes("audio/vnd.wave")]
        public async Task<ActionResult<BestMatch>> Post()
        {
            await using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var bytes = ms.ToArray();

            return await MatchFingerprintAsWav(bytes);
        }

        private async Task<ActionResult<BestMatch>> MatchFingerprintAsWav(byte[] bytes)
        {
            var tempLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
            await System.IO.File.WriteAllBytesAsync(tempLocation, bytes);

            var queryCommand = QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(tempLocation)
                .WithQueryConfig(new HighPrecisionQueryConfiguration())
                .UsingServices(_modelService, _audioService);

            var queryResult = await queryCommand.Query();

            System.IO.File.Delete(tempLocation);

            return new BestMatch
            {
                Artist = queryResult.BestMatch?.Track?.Artist,
                Title = queryResult.BestMatch?.Track?.Title
            };
        }
    }

    public class BestMatch
    {
        public string Artist { get; set; }
        public string Title { get; set; }
    }
}
