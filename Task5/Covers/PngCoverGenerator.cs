using System;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Task5.Covers.Interfaces;
using Task5.Determinism;
using Path = System.IO.Path;

namespace Task5.Covers
{
    public class PngCoverGenerator : ICoverGenerator
    {
        private const int W = 512;
        private const int H = 512;

        public byte[] GenerateCover(string songId, string locale, string songTitle, string artist, string? albumTitle)
        {
            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale.Trim();

            if (!SongId.TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                throw new ArgumentException("Invalid songId. Expected <seed>-<index>", nameof(songId));

            int detSeed = DeterministicSeed.MakeDetSeed(seed, index, locale);
            var rnd = new Random(detSeed);

            songTitle = (songTitle ?? "").Trim();
            artist = (artist ?? "").Trim();
            albumTitle = string.IsNullOrWhiteSpace(albumTitle) ? null : albumTitle.Trim();

            using var image = new Image<Rgba32>(W, H);
            var bg = MakeBackground(image, rnd);

            var baseDir = AppContext.BaseDirectory;
            string[] fontPaths =
            {
                Path.Combine(baseDir, "Resources", "Fonts", "RobotoSlab-Medium.ttf"),
                Path.Combine(baseDir, "Resources", "Fonts", "PlayfairDisplay-VariableFont_wght.ttf"),
                Path.Combine(baseDir, "Resources", "Fonts", "MontserratAlternates-Regular.ttf"),
                Path.Combine(baseDir, "Resources", "Fonts", "Kurale-Regular.ttf"),
            };

            var fontCollection = new FontCollection();
            FontFamily family = fontCollection.Add(fontPaths[rnd.Next(fontPaths.Length)]);

            string layout = PickWeighted(rnd,
                ("CenterMid", 22),
                ("CenterTop", 16),
                ("CenterBottom", 16),
                ("LeftMid", 18),
                ("LeftTop", 14),
                ("LeftBottom", 14));

            image.Mutate(ctx =>
            {
                DrawLayoutWithFallback(ctx, rnd, layout, songTitle, artist, albumTitle, family, bg);
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        private sealed class BgInfo
        {
            public bool IsGradient { get; init; }
            public Rgba32 Solid { get; init; }
            public Rgba32 A { get; init; }
            public Rgba32 B { get; init; }
        }

        private static BgInfo MakeBackground(Image<Rgba32> image, Random rnd)
        {
            var palette = new[]
            {
                "F59E0B","FB923C","F97316","F43F5E","EC4899",
                "A855F7","8B5CF6","6366F1","3B82F6","06B6D4",
                "14B8A6","22C55E","84CC16","EAB308",
                "F1F5F9","E2E8F0"
            };

            bool gradient = rnd.NextDouble() < 0.55;

            if (!gradient)
            {
                var c = Parse(palette[rnd.Next(palette.Length)]);
                c = Lighten(c, 0.10f);
                image.Mutate(ctx => ctx.Fill(c));
                return new BgInfo { IsGradient = false, Solid = c };
            }
            else
            {
                var a = Parse(palette[rnd.Next(palette.Length)]);
                var b = Parse(palette[rnd.Next(palette.Length)]);

                a = Lighten(a, 0.12f);
                b = Lighten(b, 0.12f);
                b = Mix(b, a, 0.38f);

                var start = new PointF(0, 0);
                var end = rnd.NextDouble() < 0.5 ? new PointF(W, 0) : new PointF(0, H);

                image.Mutate(ctx =>
                {
                    ctx.Fill(new LinearGradientBrush(
                        start,
                        end,
                        GradientRepetitionMode.None,
                        new ColorStop(0f, a),
                        new ColorStop(1f, b)
                    ));
                });

                return new BgInfo { IsGradient = true, A = a, B = b };
            }
        }

        private static void DrawLayoutWithFallback(
            IImageProcessingContext ctx,
            Random rnd,
            string layout,
            string songTitle,
            string artist,
            string? albumTitle,
            FontFamily family,
            BgInfo bg)
        {
            float pad = 44f;

            bool center = layout.StartsWith("Center", StringComparison.OrdinalIgnoreCase);
            bool top = layout.EndsWith("Top", StringComparison.OrdinalIgnoreCase);
            bool bottom = layout.EndsWith("Bottom", StringComparison.OrdinalIgnoreCase);

            HorizontalAlignment align = center ? HorizontalAlignment.Center : HorizontalAlignment.Left;

            RectangleF box =
                top ? new RectangleF(pad, 48f, W - pad * 2, H * 0.40f) :
                bottom ? new RectangleF(pad, H * 0.58f, W - pad * 2, H * 0.34f) :
                new RectangleF(pad, H * 0.32f, W - pad * 2, H * 0.40f);

            float gap = 16f;
            var titleBox = new RectangleF(box.X, box.Y, box.Width, box.Height * 0.62f);
            var artistBox = new RectangleF(box.X, titleBox.Bottom + gap, box.Width, box.Bottom - (titleBox.Bottom + gap));

            var approxBg = bg.IsGradient ? Mix(bg.A, bg.B, 0.5f) : bg.Solid;
            Color textColor = PickTextColor(approxBg);

            float titleStart = rnd.Next(56, 70);
            float artistStart = rnd.Next(24, 34);

            string titleToDraw = PickBestTitleNoTruncate(family, songTitle, albumTitle, titleBox, align, titleStart, maxLines: 3);

            DrawTextNoTruncate(ctx, family, titleToDraw, titleStart, FontStyle.Bold, titleBox, textColor, align, 3);

            if (!string.IsNullOrWhiteSpace(artist))
                DrawTextNoTruncate(ctx, family, artist, artistStart, FontStyle.Regular, artistBox, textColor, align, 1);
        }

        private static string PickBestTitleNoTruncate(
            FontFamily family,
            string songTitle,
            string? albumTitle,
            RectangleF box,
            HorizontalAlignment align,
            float startSize,
            int maxLines)
        {
            string s1 = (songTitle ?? "").Trim();
            string s2 = (!string.IsNullOrWhiteSpace(albumTitle) && !string.Equals(albumTitle, "Single", StringComparison.OrdinalIgnoreCase))
                ? albumTitle.Trim()
                : "";

            if (CanFitNoTruncate(family, s1, startSize, FontStyle.Bold, box, align, maxLines))
                return s1;

            if (!string.IsNullOrWhiteSpace(s2) && CanFitNoTruncate(family, s2, startSize, FontStyle.Bold, box, align, maxLines))
                return s2;

            string abbr = MakeAbbreviation(s1);
            if (CanFitNoTruncate(family, abbr, startSize, FontStyle.Bold, box, align, maxLines))
                return abbr;

            return abbr;
        }

        private static bool CanFitNoTruncate(
            FontFamily family,
            string text,
            float startSize,
            FontStyle style,
            RectangleF box,
            HorizontalAlignment align,
            int maxLines)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return true;

            const float minSize = 14f;
            float size = startSize;

            while (size >= minSize)
            {
                Font font = family.CreateFont(size, style);
                var opt = new RichTextOptions(font)
                {
                    Origin = new PointF(OriginX(box, align), box.Y),
                    WrappingLength = box.Width,
                    HorizontalAlignment = align,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var measured = TextMeasurer.MeasureSize(text, opt);
                float lineHeight = font.Size * 1.15f;
                float maxHeight = Math.Min(box.Height, lineHeight * maxLines);

                if (measured.Width <= box.Width + 0.5f && measured.Height <= maxHeight + 0.5f)
                    return true;

                size -= 2f;
            }

            return false;
        }

        private static void DrawTextNoTruncate(
            IImageProcessingContext ctx,
            FontFamily family,
            string text,
            float startSize,
            FontStyle style,
            RectangleF box,
            Color color,
            HorizontalAlignment align,
            int maxLines)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;

            const float minSize = 14f;
            float size = startSize;
            RichTextOptions opt = default;

            while (size >= minSize)
            {
                Font font = family.CreateFont(size, style);
                opt = new RichTextOptions(font)
                {
                    Origin = new PointF(OriginX(box, align), box.Y),
                    WrappingLength = box.Width,
                    HorizontalAlignment = align,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var measured = TextMeasurer.MeasureSize(text, opt);
                float lineHeight = font.Size * 1.15f;
                float maxHeight = Math.Min(box.Height, lineHeight * maxLines);

                if (measured.Width <= box.Width + 0.5f && measured.Height <= maxHeight + 0.5f)
                    break;

                size -= 2f;
            }

            Font finalFont = family.CreateFont(Math.Max(size, minSize), style);
            opt = new RichTextOptions(finalFont)
            {
                Origin = new PointF(OriginX(box, align), box.Y),
                WrappingLength = box.Width,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Top
            };

            var shadow = (color == Color.White)
                ? Color.FromRgba(0, 0, 0, 180)
                : Color.FromRgba(255, 255, 255, 150);

            var shadowOpt = opt;
            shadowOpt.Origin = new PointF(opt.Origin.X + 2, opt.Origin.Y + 2);

            ctx.DrawText(shadowOpt, text, shadow);
            ctx.DrawText(opt, text, color);
        }

        private static string MakeAbbreviation(string title)
        {
            title = (title ?? "").Trim();
            if (title.Length == 0) return "—";

            int paren = title.IndexOf('(');
            if (paren > 0) title = title.Substring(0, paren).Trim();

            var tokens = title
                .Split(new[] { ' ', '\t', '-', '—', '/', '\\', ':', ';', ',', '.', '!', '?', '[', ']', '{', '}', '"' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToArray();

            if (tokens.Length == 0) return "—";
            if (tokens.Length == 1)
            {
                string one = tokens[0];
                if (one.Length <= 10) return one.ToUpperInvariant();
                return one.Substring(0, 10).ToUpperInvariant();
            }

            string[] stop =
            {
                "the","a","an","of","and","or","to","in","on","at","for","from","into","over","under","between","within"
            };

            var sb = new StringBuilder();
            foreach (var t in tokens)
            {
                var low = t.ToLowerInvariant();
                if (stop.Contains(low)) continue;

                char c = t[0];
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpperInvariant(c));

                if (sb.Length >= 6) break;
            }

            if (sb.Length == 0)
            {
                foreach (var t in tokens.Take(6))
                {
                    char c = t[0];
                    if (char.IsLetterOrDigit(c))
                        sb.Append(char.ToUpperInvariant(c));
                    if (sb.Length >= 6) break;
                }
            }

            return sb.Length == 0 ? "—" : sb.ToString();
        }

        private static Color PickTextColor(Rgba32 bg)
        {
            float r = bg.R / 255f;
            float g = bg.G / 255f;
            float b = bg.B / 255f;
            float luma = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            return luma > 0.62f ? Color.FromRgb(18, 18, 18) : Color.White;
        }

        private static float OriginX(RectangleF box, HorizontalAlignment align) =>
            align switch
            {
                HorizontalAlignment.Center => box.X + box.Width * 0.5f,
                HorizontalAlignment.Right => box.Right,
                _ => box.X
            };

        private static string PickWeighted(Random rnd, params (string id, int w)[] items)
        {
            int sum = 0;
            for (int i = 0; i < items.Length; i++) sum += items[i].w;
            int roll = rnd.Next(0, sum);
            int acc = 0;
            for (int i = 0; i < items.Length; i++)
            {
                acc += items[i].w;
                if (roll < acc) return items[i].id;
            }
            return items[0].id;
        }

        private static Rgba32 Parse(string hex) => Color.ParseHex(hex).ToPixel<Rgba32>();

        private static Rgba32 Mix(Rgba32 a, Rgba32 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            byte r = (byte)Math.Clamp(a.R + (b.R - a.R) * t, 0, 255);
            byte g = (byte)Math.Clamp(a.G + (b.G - a.G) * t, 0, 255);
            byte bl = (byte)Math.Clamp(a.B + (b.B - a.B) * t, 0, 255);
            return new Rgba32(r, g, bl, 255);
        }

        private static Rgba32 Lighten(Rgba32 c, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            byte r = (byte)Math.Clamp(c.R + (255 - c.R) * amount, 0, 255);
            byte g = (byte)Math.Clamp(c.G + (255 - c.G) * amount, 0, 255);
            byte b = (byte)Math.Clamp(c.B + (255 - c.B) * amount, 0, 255);
            return new Rgba32(r, g, b, 255);
        }
    }
}
