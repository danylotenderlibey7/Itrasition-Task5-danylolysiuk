using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Task5.Covers.Interfaces;
using Task5.Determinism;

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

            albumTitle = string.IsNullOrWhiteSpace(albumTitle) ? null : albumTitle.Trim();
            string mainTitle =
                !string.IsNullOrWhiteSpace(albumTitle) &&
                !string.Equals(albumTitle, "Single", StringComparison.OrdinalIgnoreCase)
                    ? albumTitle
                    : songTitle;

            using var image = new Image<Rgba32>(W, H);
            var bg = MakeBackground(image, rnd);

            var baseDir = AppContext.BaseDirectory;
            string[] fontPaths =
            {
                System.IO.Path.Combine(baseDir, "Resources", "Fonts", "RobotoSlab-Medium.ttf"),
                System.IO.Path.Combine(baseDir, "Resources", "Fonts", "PlayfairDisplay-VariableFont_wght.ttf"),
                System.IO.Path.Combine(baseDir, "Resources", "Fonts", "MontserratAlternates-Regular.ttf"),
                System.IO.Path.Combine(baseDir, "Resources", "Fonts", "Kurale-Regular.ttf"),
            };

            var fontCollection = new FontCollection();
            FontFamily family = fontCollection.Add(fontPaths[rnd.Next(fontPaths.Length)]);

            string layout = PickWeighted(rnd,
                ("CenterTop", 16),
                ("CenterMiddle", 18),
                ("CenterBottom", 16),
                ("LeftTop", 16),
                ("LeftMiddle", 18),
                ("LeftBottom", 16));

            image.Mutate(ctx =>
            {
                DrawLayout(ctx, rnd, layout, mainTitle, artist, family, bg);
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
                c = Lighten(c, 0.08f);
                image.Mutate(ctx => ctx.Fill(c));
                return new BgInfo { IsGradient = false, Solid = c };
            }
            else
            {
                var a = Parse(palette[rnd.Next(palette.Length)]);
                var b = Parse(palette[rnd.Next(palette.Length)]);

                a = Lighten(a, 0.10f);
                b = Lighten(b, 0.10f);
                b = Mix(b, a, 0.35f);

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

        private static void DrawLayout(
            IImageProcessingContext ctx,
            Random rnd,
            string layout,
            string title,
            string artist,
            FontFamily family,
            BgInfo bg)
        {
            float padX = 46f;
            float padY = 46f;

            bool center = layout.StartsWith("Center", StringComparison.OrdinalIgnoreCase);
            var align = center ? HorizontalAlignment.Center : HorizontalAlignment.Left;

            float boxH = 220f;
            float boxW = W - padX * 2;

            float y = layout.EndsWith("Top", StringComparison.OrdinalIgnoreCase)
                ? padY
                : layout.EndsWith("Bottom", StringComparison.OrdinalIgnoreCase)
                    ? (H - padY - boxH)
                    : (H - boxH) * 0.5f;

            var box = new RectangleF(padX, y, boxW, boxH);

            float gap = 16f;
            float artistH = 62f;

            var titleBox = new RectangleF(box.X, box.Y, box.Width, Math.Max(60f, box.Height - artistH - gap));
            var artistBox = new RectangleF(box.X, titleBox.Bottom + gap, box.Width, artistH);

            var approxBg = bg.IsGradient ? Mix(bg.A, bg.B, 0.5f) : bg.Solid;
            Color text = PickTextColor(approxBg);

            float titleStart = rnd.Next(62, 78);
            float artistStart = rnd.Next(28, 38);

            FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, titleBox, text, align, 2, verticalCenter: center);
            FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, artistBox, text, align, 1, verticalCenter: center);
        }

        private static Color PickTextColor(Rgba32 bg)
        {
            float r = bg.R / 255f;
            float g = bg.G / 255f;
            float b = bg.B / 255f;
            float luma = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            return luma > 0.62f ? Color.FromRgb(18, 18, 18) : Color.White;
        }

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

        private static float OriginX(RectangleF box, HorizontalAlignment align) =>
            align switch
            {
                HorizontalAlignment.Center => box.X + box.Width * 0.5f,
                HorizontalAlignment.Right => box.Right,
                _ => box.X
            };

        private static void FitTextInBox(
            IImageProcessingContext ctx,
            FontFamily family,
            string text,
            float startSize,
            FontStyle style,
            RectangleF box,
            Color color,
            HorizontalAlignment align,
            int maxLines,
            bool verticalCenter)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;

            float size = startSize;
            const float minSize = 14f;

            RichTextOptions opt = default;
            string candidate = text;

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

                float lineHeight = font.Size * 1.18f;
                float maxHeight = Math.Min(box.Height, lineHeight * maxLines);

                candidate = TruncateToFitHeight(text, opt, maxHeight);
                var measured = TextMeasurer.MeasureSize(candidate, opt);

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

            float allowedHeight = Math.Min(box.Height, finalFont.Size * 1.18f * maxLines);
            candidate = TruncateToFitHeight(text, opt, allowedHeight);

            var measuredFinal = TextMeasurer.MeasureSize(candidate, opt);

            float y = box.Y;
            if (verticalCenter)
                y = box.Y + Math.Max(0f, (box.Height - measuredFinal.Height) * 0.5f);

            opt.Origin = new PointF(OriginX(box, align), y);

            var shadow = (color == Color.White)
                ? Color.FromRgba(0, 0, 0, 200)
                : Color.FromRgba(255, 255, 255, 170);

            var shadowOpt = opt;
            shadowOpt.Origin = new PointF(opt.Origin.X + 2.5f, opt.Origin.Y + 2.5f);

            ctx.DrawText(shadowOpt, candidate, shadow);
            ctx.DrawText(opt, candidate, color);
        }

        private static string TruncateToFitHeight(string text, RichTextOptions opt, float maxHeight)
        {
            if (TextMeasurer.MeasureSize(text, opt).Height <= maxHeight)
                return text;

            int lo = 0;
            int hi = text.Length;
            while (lo + 1 < hi)
            {
                int mid = (lo + hi) / 2;
                string t = text.Substring(0, mid).TrimEnd();
                if (TextMeasurer.MeasureSize(t, opt).Height <= maxHeight)
                    lo = mid;
                else
                    hi = mid;
            }
            return text.Substring(0, lo).TrimEnd();
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
