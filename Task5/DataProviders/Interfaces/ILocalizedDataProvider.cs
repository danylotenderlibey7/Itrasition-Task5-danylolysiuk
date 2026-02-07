namespace Task5.DataProviders.Interfaces
{
    public interface ILocalizedDataProvider
    {
        string GetSongTitle();
        string GetArtistName();
        string GetAlbumTitle();
        string GetGenre();

        string GetReview(string songTitle, string artistName, string albumTitle, string genre);
    }
}
