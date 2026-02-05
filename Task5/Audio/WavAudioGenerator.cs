using NAudio.Wave;
using Task5.Audio.Interfaces;
using Task5.Determinism;

namespace Task5.Audio
{
    public class WavAudioGenerator : IAudioGenerator
    {
        public byte[] GeneratePreviewWav(string songId, string locale, int durationSeconds = 6)
        {
            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale.Trim();

            if (!SongId.TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                throw new ArgumentException("Invalid songId. Expected <seed>-<index>", nameof(songId));

            using var memoryStream = new MemoryStream();
            var format = new WaveFormat(44100, 16, 1);

            int detSeed = DeterministicSeed.MakeDetSeed(seed, index, locale);
            var rnd = new Random(detSeed);

            using (var writer = new WaveFileWriter(memoryStream,format))
            {
                int sampleRate = 44100;
                double amplitude = 0.2;
                double noteDuration = 0.5;

                int notesCount = Math.Max(1,(int)(durationSeconds / noteDuration));

                double[] notes = new double[notesCount];
                for (int i = 0; i < notesCount; i++) 
                {
                    notes[i] = rnd.Next(220, 881);
                }

                int samplesPerNote = (int)(sampleRate * noteDuration);
                int totalSamples = sampleRate * durationSeconds;

                for (int n = 0; n < totalSamples; n++)
                {
                    int noteIndex = (n / samplesPerNote) % notes.Length;
                    double frequency = notes[noteIndex];

                    var t = (double)n / sampleRate;
                    var value = Math.Sin(2 * Math.PI * frequency * t);

                    short sample = (short)(value * amplitude * short.MaxValue);

                    writer.WriteByte((byte)(sample & 0xff));
                    writer.WriteByte((byte)((sample >> 8) & 0xff));
                }
            }

            return memoryStream.ToArray();
        }
    }
}
