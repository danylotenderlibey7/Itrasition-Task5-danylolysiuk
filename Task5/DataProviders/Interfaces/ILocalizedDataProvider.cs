namespace Task5.DataProviders.Interfaces
{
    public interface ILocalizedDataProvider
    {
        string GetSongTitle();
        string GetArtistName();
        string GetAlbumTitle();
        string GetGenre();
    }
}
