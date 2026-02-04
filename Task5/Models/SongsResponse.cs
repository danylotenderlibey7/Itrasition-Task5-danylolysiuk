namespace Task5.Models
{
    public class SongsResponse
    {
        public List<Song> Songs { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
