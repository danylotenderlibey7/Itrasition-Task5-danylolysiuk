namespace Task5.Models
{
    public class Song
    {
        public string? Id { get; set; } = String.Empty;
        public int Index { get; set; }
        public string? SongTitle { get; set; }
        public string? Artist { get; set; }
        public string? AlbumTitle { get; set; }
        public string? Genre { get; set; }
        public int Likes { get; set; }

    }
}
