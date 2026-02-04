using Microsoft.AspNetCore.Mvc;
using Task5.Generators.Interfaces;
using Task5.Models;

namespace Task5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly ISongGenerator _songGenerator;
        public SongsController(ISongGenerator songGenerator)
        {
            _songGenerator = songGenerator;
        }

        [HttpGet]
        public IActionResult GetSongs([FromQuery] SongsRequest request)
        {
            var songs = _songGenerator.GenerateSongs(request);
            var response = new SongsResponse
            {
                Songs = songs,
                Page = request.Page,
                PageSize = request.PageSize
            };
            return Ok(response);
        }
    }
}

