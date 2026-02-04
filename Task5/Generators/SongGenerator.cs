using Task5.DataProviders;
using Task5.Generators.Interfaces;
using Task5.Models;

namespace Task5.Generators
{
    public class SongGenerator : ISongGenerator
    {
        public List<Song> GenerateSongs(SongsRequest request)
        {
            var songs = new List<Song>();
            var locale = string.IsNullOrWhiteSpace(request.Locale) ? "en-US" : request.Locale.Trim();
            int startIndex = (request.Page - 1) * request.PageSize;

            for (int i = 0; i < request.PageSize; i++)
            {
                int index = startIndex + i + 1;

                int recordSeed = MakeSeed(request.Seed, index, locale);
                var recordRnd = new Random(recordSeed);
                var provider = new BogusDataProvider(locale, recordRnd);

                var song = new Song
                {
                    Id = $"{request.Seed}-{index}",
                    Index = index,
                    SongTitle = provider.GetSongTitle(),
                    Artist = provider.GetArtistName(),
                    AlbumTitle = provider.GetAlbumTitle(),
                    Genre = provider.GetGenre(),
                    Likes = GenerateLikes(request.LikesAvg, recordRnd)
                };

                songs.Add(song);
            }


            return songs;
        }

        private int GenerateLikes(double likesAvg, Random rnd)
        {
            likesAvg = Math.Clamp(likesAvg, 0, 10);
            int baseLikes = (int)Math.Floor(likesAvg);
            double probalitty = likesAvg - baseLikes;

            if(rnd.NextDouble()<probalitty)
            {
                baseLikes++;
            }

            return Math.Clamp(baseLikes, 0, 10);
        }

        private static int MakeSeed(ulong seed, int index, string locale)
        {
            unchecked
            {
                int h = (int)(seed ^ (seed >> 32));
                h = (h * 397) ^ index;
                h = (h * 397) ^ (locale?.GetHashCode() ?? 0);
                return h & 0x7FFFFFFF;
            }
        }

    }

}
