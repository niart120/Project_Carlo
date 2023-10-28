namespace NSMBRNG.NSMBLCG
{
    public static class NSMBLCGExtension
    {
        public static uint NextSeed(this uint seed)
        {
            ulong seed64 = seed;
            seed64 = seed64 * 0x19660DUL + 0x3C6EF35FUL;
            seed64 = ((seed64 >> 32) + seed64) & 0xFFFFFFFFUL;
            return (uint)seed64;
        }

        public static uint Advance(ref this uint seed)
        {
            ulong seed64 = seed;
            seed64 = seed64 * 0x19660DUL + 0x3C6EF35FUL;
            seed64 = ((seed64 >> 32) + seed64) & 0xFFFFFFFFUL;
            seed = (uint)seed64;
            return seed;
        }

        public static uint GetRand(ref this uint seed, uint m)
        {
            var r = seed.Advance() >> 19;
            return (m * (r & 0xFFF)) >> 12;

        }
    }
}
