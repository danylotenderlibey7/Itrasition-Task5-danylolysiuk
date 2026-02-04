namespace Task5.Covers.Interfaces
{
    public interface ICoverGenerator
    {
        byte[] GenerateCover(string songId, string locale, string songTitle,string artist, string? albumTitle);
    }
}
