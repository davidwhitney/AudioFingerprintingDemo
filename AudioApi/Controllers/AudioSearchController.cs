using System;
using System.IO;
using System.Threading.Tasks;
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
        [Consumes("audio/vnd.wave")]
        public async Task<ActionResult<BestMatch>> Post()
        {
            await using var ms = new MemoryStream(2048);
            await Request.Body.CopyToAsync(ms);

            var tempLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
            await System.IO.File.WriteAllBytesAsync(tempLocation, ms.ToArray());

            var queryCommand = QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(tempLocation)
                .WithQueryConfig(new HighPrecisionQueryConfiguration())
                .UsingServices(_modelService, _audioService);

            var queryResult = await queryCommand.Query();

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
