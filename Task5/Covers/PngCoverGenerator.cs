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

        public byte[] GenerateCover(
            string songId, string locale, string songTitle, string artist, string? albumTitle)
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
            ApplyRandomBackground(image, rnd);

            string[] fontPaths =
            {
                "Resources/Fonts/PlaypenSans-Regular.ttf",
                "Resources/Fonts/RobotoSlab-Medium.ttf",
                "Resources/Fonts/BadScript-Regular.ttf",
                "Resources/Fonts/Kurale-Regular.ttf",
                "Resources/Fonts/MontserratAlternates-Regular.ttf",
                "Resources/Fonts/Pacifico-Regular.ttf",
                "Resources/Fonts/PlayfairDisplay-VariableFont_wght.ttf",
                "Resources/Fonts/RubikDoodleShadow-Regular.ttf"
            };

            var fontCollection = new FontCollection();
            FontFamily family = fontCollection.Add(fontPaths[rnd.Next(fontPaths.Length)]);

            Color textColor = rnd.NextDouble() < 0.20
                ? Color.ParseHex("F1F5F9")
                : Color.White;

            string layout = PickWeighted(
                rnd,
                ("BottomStack", 30),
                ("TopTitleBottomArtist", 22),
                ("Centered", 18),
                ("SplitLeft", 16),
                ("SplitRight", 14));

            image.Mutate(ctx =>
            {
                if (rnd.NextDouble() < 0.75)
                    ApplyReadabilityOverlay(ctx, rnd);

                DrawLayout(ctx, rnd, layout, mainTitle, artist, family, textColor);
            });

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
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

        private static void ApplyRandomBackground(Image<Rgba32> image, Random rnd)
        {
            var palette = new[]
            {
                "0F172A","1E3A8A","4F46E5","7C3AED","A855F7","D946EF",
                "C026D3","DB2777","DC2626","C2410C","EA580C","F59E0B",
                "CA8A04","65A30D","16A34A","0D9488","0891B2","0284C7",
                "075985","4338CA","312E81","0B1320","EF4444","BE123C",
                "F97316","D97706","059669","065F46","14B8A6","0891B2"
            };

            var c1 = Color.ParseHex(palette[rnd.Next(palette.Length)]);
            var c2 = Color.ParseHex(palette[rnd.Next(palette.Length)]);
            var c3 = Color.ParseHex(palette[rnd.Next(palette.Length)]);
            if (c1 == c2) c2 = Color.ParseHex(palette[(Array.IndexOf(palette, palette[rnd.Next(palette.Length)]) + 1) % palette.Length]);

            int type = rnd.Next(0, 5);

            image.Mutate(ctx =>
            {
                switch (type)
                {
                    case 0:
                        {
                            var start = new PointF(rnd.NextSingle() * W, rnd.NextSingle() * H);
                            var end = new PointF(rnd.NextSingle() * W, rnd.NextSingle() * H);
                            ctx.Fill(new LinearGradientBrush(
                                start,
                                end,
                                GradientRepetitionMode.None,
                                new ColorStop(0f, c1),
                                new ColorStop(rnd.NextSingle() * 0.6f + 0.2f, c2),
                                new ColorStop(1f, c3)));
                            break;
                        }
                    case 1:
                        {
                            var center = new PointF(rnd.NextSingle() * W, rnd.NextSingle() * H);
                            float radius = (float)(Math.Min(W, H) * (0.4 + rnd.NextDouble() * 0.6));
                            ctx.Fill(new RadialGradientBrush(
                            center,
                            radius,
                            GradientRepetitionMode.None,
                            new ColorStop(0f, c1),
                            new ColorStop(0.7f, c2),
                            new ColorStop(1f, c3)
                        ));
                            break;

                        }
                    case 2:
                        {
                            if (rnd.Next(0, 2) == 0)
                            {
                                ctx.Fill(c1, new RectangleF(0, 0, W * 0.5f, H));
                                ctx.Fill(c2, new RectangleF(W * 0.5f, 0, W * 0.5f, H));
                            }
                            else
                            {
                                ctx.Fill(c1, new RectangleF(0, 0, W, H * 0.5f));
                                ctx.Fill(c2, new RectangleF(0, H * 0.5f, W, H * 0.5f));
                            }
                            break;
                        }
                    case 3:
                        {
                            ctx.Fill(c1);
                            break;
                        }
                    default:
                        {
                            ctx.Fill(new LinearGradientBrush(
                                new PointF(0, 0),
                                new PointF(W, H),
                                GradientRepetitionMode.None,
                                new ColorStop(0f, c2),
                                new ColorStop(1f, c1)));
                            break;
                        }
                }
            });
        }

        private static void ApplyReadabilityOverlay(IImageProcessingContext ctx, Random rnd)
        {
            int kind = rnd.Next(0, 5);
            if (kind == 0)
            {
                var o = new LinearGradientBrush(
                    new PointF(0, H),
                    new PointF(0, 0),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, Color.FromRgba(0, 0, 0, 150)),
                    new ColorStop(0.60f, Color.FromRgba(0, 0, 0, 0))
                );
                ctx.Fill(o);
                return;
            }
            if (kind == 1)
            {
                var o = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(0, H),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, Color.FromRgba(0, 0, 0, 125)),
                    new ColorStop(0.52f, Color.FromRgba(0, 0, 0, 0)),
                    new ColorStop(1f, Color.FromRgba(0, 0, 0, 105))
                );
                ctx.Fill(o);
                return;
            }
            if (kind == 2)
            {
                var o = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(W, 0),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, Color.FromRgba(0, 0, 0, 125)),
                    new ColorStop(0.55f, Color.FromRgba(0, 0, 0, 0))
                );
                ctx.Fill(o);
                return;
            }
            if (kind == 3)
            {
                var o = new LinearGradientBrush(
                    new PointF(W, 0),
                    new PointF(0, H),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, Color.FromRgba(0, 0, 0, 95)),
                    new ColorStop(0.50f, Color.FromRgba(0, 0, 0, 0)),
                    new ColorStop(1f, Color.FromRgba(0, 0, 0, 115))
                );
                ctx.Fill(o);
                return;
            }
            var o4 = new LinearGradientBrush(
                new PointF(0, 0),
                new PointF(W, H),
                GradientRepetitionMode.None,
                new ColorStop(0f, Color.FromRgba(0, 0, 0, 80)),
                new ColorStop(0.55f, Color.FromRgba(0, 0, 0, 0)),
                new ColorStop(1f, Color.FromRgba(0, 0, 0, 95))
            );
            ctx.Fill(o4);
        }

        private static void DrawLayout(
            IImageProcessingContext ctx,
            Random rnd,
            string layout,
            string title,
            string artist,
            FontFamily family,
            Color textColor)
        {
            float pad = 36f;

            var top = new RectangleF(pad, pad, W - pad * 2, H * 0.34f - pad);
            var mid = new RectangleF(pad, H * 0.36f, W - pad * 2, H * 0.30f);
            var bot = new RectangleF(pad, H * 0.70f, W - pad * 2, H * 0.30f - pad);

            float titleStart = rnd.Next(40, 70);
            float artistStart = rnd.Next(18, 32);

            if (layout == "BottomStack")
            {
                var b1 = new RectangleF(bot.X, bot.Y, bot.Width, bot.Height * 0.64f);
                var b2 = new RectangleF(bot.X, bot.Y + bot.Height * 0.64f, bot.Width, bot.Height * 0.36f);

                FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, b1, textColor, HorizontalAlignment.Left, 3);
                FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, b2, textColor, HorizontalAlignment.Left, 1);
                return;
            }
            if (layout == "TopTitleBottomArtist")
            {
                FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, top, textColor, HorizontalAlignment.Left, 3);
                FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, bot, textColor, HorizontalAlignment.Left, 2);
                return;
            }
            if (layout == "Centered")
            {
                var c1 = new RectangleF(mid.X, mid.Y, mid.Width, mid.Height * 0.64f);
                var c2 = new RectangleF(mid.X, mid.Y + mid.Height * 0.64f, mid.Width, mid.Height * 0.36f);

                FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, c1, textColor, HorizontalAlignment.Center, 3);
                FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, c2, textColor, HorizontalAlignment.Center, 2);
                return;
            }
            if (layout == "SplitLeft")
            {
                var t = new RectangleF(pad, H * 0.16f, W * 0.66f - pad, H * 0.36f);
                var a = new RectangleF(pad, H * 0.72f, W * 0.66f - pad, H * 0.20f);

                FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, t, textColor, HorizontalAlignment.Left, 3);
                FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, a, textColor, HorizontalAlignment.Left, 1);
                return;
            }
            if (layout == "SplitRight")
            {
                var t = new RectangleF(W * 0.34f, H * 0.16f, W * 0.66f - pad, H * 0.36f);
                var a = new RectangleF(W * 0.34f, H * 0.72f, W * 0.66f - pad, H * 0.20f);

                FitTextInBox(ctx, family, title, titleStart, FontStyle.Bold, t, textColor, HorizontalAlignment.Right, 3);
                FitTextInBox(ctx, family, artist, artistStart, FontStyle.Regular, a, textColor, HorizontalAlignment.Right, 1);
            }
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
            int maxLines)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;

            float size = startSize;
            const float minSize = 14f;
            string candidate = text;

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

                var measured = TextMeasurer.MeasureSize(candidate, opt);
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

            float allowedHeight = Math.Min(box.Height, finalFont.Size * 1.15f * maxLines);
            candidate = TruncateToFitHeight(candidate, opt, allowedHeight);

            var shadow = Color.FromRgba(0, 0, 0, 140);
            var shadowOpt = opt;
            shadowOpt.Origin = new PointF(opt.Origin.X + 2, opt.Origin.Y + 2);

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
    }
}
