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
            if (_rnd.NextDouble() < 0.4)
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
        private string ApplyTemplate(string template)
        {
            return template
                .Replace("{Adjective}", _data.Adjectives[_rnd.Next(_data.Adjectives.Count)])
                .Replace("{Noun}", _data.Nouns[_rnd.Next(_data.Nouns.Count)]);
        }

    }
}
