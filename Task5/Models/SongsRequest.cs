public class SongsRequest
{
    public string Locale { get; set; } = "en-US";
    public ulong Seed { get; set; }
    public double LikesAvg { get; set; } = 5;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
