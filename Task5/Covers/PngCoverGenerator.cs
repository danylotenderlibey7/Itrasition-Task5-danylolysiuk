using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Task5.Covers.Interfaces;

namespace Task5.Covers
{
    public class PngCoverGenerator : ICoverGenerator
    {
        public byte[] GenerateCover(string songId,string locale, string songTitle, string artist, string? albumTitle)
        {
            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale.Trim();

            if (!TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                throw new ArgumentException("Invalid songId. Expected <seed>-<index>", nameof(songId));

            int detSeed = MakeDetSeed(seed, index, locale);
            var rnd = new Random(detSeed);

            byte r = (byte)rnd.Next(0, 255);
            byte g = (byte)rnd.Next(0, 255);
            byte b = (byte)rnd.Next(0, 255);

            var color = new Rgba32(r, g, b);
            using var image = new Image<Rgba32>(512, 512, color);

            string[] fontPaths =
            {
                "Resources/Fonts/PlaypenSans-Regular.ttf",
                "Resources/Fonts/RobotoSlab-Medium.ttf"
            };

            int fontIndex = rnd.Next(fontPaths.Length);
            string fontPath = fontPaths[fontIndex];

            var fontCollection = new FontCollection();
            FontFamily fontFamily = fontCollection.Add(fontPath);

            Font titleFont = fontFamily.CreateFont(56);
            Font artistFont = fontFamily.CreateFont(28);

            string mainTitle;

            albumTitle = string.IsNullOrWhiteSpace(albumTitle) ? null : albumTitle.Trim();
            if (!string.IsNullOrWhiteSpace(albumTitle) && 
                !string.Equals(albumTitle, "Single", StringComparison.OrdinalIgnoreCase))
            {
                mainTitle = albumTitle;
            }
            else
            {
                mainTitle = songTitle;
            }

            float paddingX = 32;
            float paddingBottom = 32;

            float yArtist = 512 - paddingBottom - 28;
            float yTitle = yArtist - 60;

            image.Mutate(ctx =>
            {
                ctx.DrawText(
                    mainTitle,
                    titleFont,
                    Color.White,
                    new PointF(paddingX, yTitle)
                );

                ctx.DrawText(
                    artist,
                    artistFont,
                    Color.White,
                    new PointF(paddingX, yArtist)
                );
            });

            using var memoryStream = new MemoryStream();

            image.SaveAsPng(memoryStream);
            return memoryStream.ToArray();

        }
        private static bool TryParseSongId(string songId, out ulong seed, out int index)
        {
            seed = default;
            index = default;

            var parts = songId.Split('-', 2);
            if (parts.Length != 2) return false;

            return ulong.TryParse(parts[0], out seed)
                && int.TryParse(parts[1], out index);
        }
        private static int MakeDetSeed(ulong seed, int index, string locale)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + seed.GetHashCode();
                h = h * 31 + index;
                foreach (var c in locale)
                    h = h * 31 + c;
                return h & 0x7FFFFFFF;
            }
        }
    }
}
