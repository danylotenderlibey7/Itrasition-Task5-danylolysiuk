using Microsoft.AspNetCore.Mvc;
using Task5.Covers.Interfaces;
using Task5.Generators.Interfaces;
using Task5.Models;
using System.Linq;

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
        public IActionResult GetSongCover([FromRoute] string songId, [FromQuery] string? locale)
        {
            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale.Trim();

            if (!TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                return BadRequest("Invalid songId format. Expected: <seed>-<index>");

            var generateRequest = new SongsRequest
            {
                Seed = seed,
                Locale = locale,
                Page = 1,
                PageSize = index
            };

            var songs = _songGenerator.GenerateSongs(generateRequest);
            var song = songs[^1];

            var bytes = _coverGenerator.GenerateCover(
                song.Id ?? $"{seed}-{index}",
                locale,
                song.SongTitle ?? "Unknown song",
                song.Artist ?? "Unknown artist",
                song.AlbumTitle
            );

            return File(bytes, "image/png");
        }

        private static bool TryParseSongId(string songId,out ulong seed,out int index)
        {
            seed = default;
            index = default;

            var parts = songId.Split('-', 2);
            if (parts.Length != 2) return false;

            return ulong.TryParse(parts[0], out seed)
                && int.TryParse(parts[1], out index);
        }
    }
}
