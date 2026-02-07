using Task5.DataProviders.Interfaces;
using System.Text.Json;
using Bogus;

namespace Task5.DataProviders
{
    public class BogusDataProvider : ILocalizedDataProvider
    {
        private readonly LocaleData _data;
        private readonly Random _rnd;
        private readonly Faker _faker;

        public BogusDataProvider(string locale, Random rnd)
        {
            string path = $"Resources/Locales/{locale}.json";
            string json = File.ReadAllText(path);

            string bogusLocale = locale switch
            {
                "en-US" => "en",
                "en" => "en",
                "uk-UA" => "uk",
                "uk" => "uk",
                _ => "en"
            };

            _rnd = rnd;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _data = JsonSerializer.Deserialize<LocaleData>(json, options)!;

            _faker = new Faker(bogusLocale);
            _faker.Random = new Randomizer(rnd.Next());
        }
        public string GetSongTitle()
        {
            var template = _data.AlbumTemplates[_rnd.Next(_data.AlbumTemplates.Count)];
            return ApplyTemplate(template);
        }
        public string GetGenre()
        {
            var template = _data.Genres[_rnd.Next(_data.Genres.Count)];
            return template;
        }
        public string GetAlbumTitle()
        {
            if (_rnd.NextDouble() < 0.3)
                return "Single";

            var template = _data.AlbumTemplates[_rnd.Next(_data.AlbumTemplates.Count)];
            return ApplyTemplate(template);
        }
        public string GetArtistName()
        {
            if (_rnd.NextDouble() < 0.5)
                return _faker.Name.FullName();
            else
                return _faker.Company.CompanyName();
        }
        public string GetReview(string songTitle, string artistname, string albumTitle, string genre)
        {
            if (_data.ReviewTemplates == null || _data.ReviewTemplates.Count == 0)
                return string.Empty;

            var template = _data.ReviewTemplates[_rnd.Next(_data.ReviewTemplates.Count)];
            template = template.Replace("{Song}", songTitle)
                       .Replace("{Artist}", artistname)
                       .Replace("{Album}", albumTitle)
                       .Replace("{Genre}", genre);

            return ApplyTemplate(template);
        }
        private string ApplyTemplate(string template)
        {
            string result = template;

            while (result.Contains("{Adjective}"))
            {
                var adj = _data.Adjectives[_rnd.Next(_data.Adjectives.Count)];
                result = ReplaceFirst(result, "{Adjective}", adj);
            }

            while (result.Contains("{Noun}"))
            {
                var noun = _data.Nouns[_rnd.Next(_data.Nouns.Count)];
                result = ReplaceFirst(result, "{Noun}", noun);
            }

            return result;
        }
        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0) return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

    }
}
