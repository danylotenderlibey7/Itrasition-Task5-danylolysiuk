using Task5.Models;

namespace Task5.Generators.Interfaces
{
    public interface ISongGenerator
    {
        List<Song> GenerateSongs(SongsRequest request);
    }
}