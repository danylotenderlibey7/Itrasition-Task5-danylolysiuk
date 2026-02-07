using Microsoft.AspNetCore.Mvc;
using Task5.Audio.Interfaces;

namespace Task5.Controllers
{
    [ApiController]
    [Route("api/songs")]
    public class SongsAudioController : ControllerBase
    {
        private readonly IAudioGenerator _audioGenerator;
        public SongsAudioController(IAudioGenerator audioGenerator)
        {
            _audioGenerator = audioGenerator;
        }

        [HttpGet("{songId}/preview")]
        public IActionResult GetSongAudio([FromRoute] string songId, [FromQuery] string? locale)
        {
            var bytes = _audioGenerator.GeneratePreviewWav(songId, locale ?? "en-US");
            return File(bytes,"audio/wav", enableRangeProcessing: true);
        }
    }
}
