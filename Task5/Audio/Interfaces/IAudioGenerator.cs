namespace Task5.Audio.Interfaces
{
    public interface IAudioGenerator
    {
        byte[] GeneratePreviewWav(string songId, string locale, int durationSeconds = 6);
    }
}
