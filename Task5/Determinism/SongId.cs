namespace Task5.Determinism
{
    public class SongId
    {
        public static bool TryParseSongId(string songId, out ulong seed, out int index)
        {
            seed = default;
            index = default;

            var parts = songId.Split('-', 2);
            if (parts.Length != 2) return false;

            return ulong.TryParse(parts[0], out seed)
                && int.TryParse(parts[1], out index);
        }
    }
}
