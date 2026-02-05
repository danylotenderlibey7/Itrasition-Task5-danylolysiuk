using Microsoft.AspNetCore.Mvc;
using Task5.Covers.Interfaces;
using Task5.Generators.Interfaces;
using Task5.Models;
using System.Linq;
using Task5.Determinism;

namespace Task5.Controllers
{
    [ApiController]
    [Route("api/songs")]
    public class SongsCoverController : ControllerBase
    {
        private readonly ICoverGenerator _coverGenerator;
        private readonly ISongGenerator _songGenerator;

        public SongsCoverController(ICoverGenerator coverGenerator, ISongGenerator songGenerator)
        {
            _coverGenerator = coverGenerator;
            _songGenerator = songGenerator;
        }

        [HttpGet("{songId}/cover")]
        public IActionResult GetSongCover([FromRoute] string songId, [FromQuery] SongsRequest request)
        {
            request.Locale = string.IsNullOrWhiteSpace(request.Locale) ? "en-US" : request.Locale.Trim();

            if (!SongId.TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                return BadRequest("Invalid songId format. Expected: <seed>-<index>");

            var generateRequest = new SongsRequest
            {
                Seed = seed,
                Locale = request.Locale,
                Page = 1,
                PageSize = index
            };

            var songs = _songGenerator.GenerateSongs(generateRequest);
            var song = songs[^1];

            var bytes = _coverGenerator.GenerateCover(
                song.Id ?? $"{seed}-{index}",
                request.Locale,
                song.SongTitle ?? "Unknown song",
                song.Artist ?? "Unknown artist",
                song.AlbumTitle
            );

            return File(bytes, "image/png");
        }
    }
}
