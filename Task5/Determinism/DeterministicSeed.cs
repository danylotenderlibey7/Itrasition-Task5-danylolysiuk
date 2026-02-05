namespace Task5.Determinism
{
    public static class DeterministicSeed
    {
        public static int MakeDetSeed(ulong seed, int index, string locale)
        {
            unchecked
            {
                int h = (int)(seed ^ (seed >> 32));
                h = (h * 397) ^ index;
                h = (h * 397) ^ StableStringHashUtf8(locale);
                return h & 0x7FFFFFFF;
            }
        }
        private static int StableStringHashUtf8(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            const uint fnvOffsetBasis = 2166136261;
            const uint fnvPrime = 16777619;

            uint hash = fnvOffsetBasis;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= fnvPrime;
            }

            return (int)(hash & 0x7FFFFFFF);
        }

    }
}
