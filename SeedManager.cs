using System;

namespace OrbRandomizer
{
    public static class SeedManager
    {
        public static int Seed { get; private set; }
        public static Random Random { get; private set; }
        public static void SetSeed(int seed)
        {
            Seed = seed;
            Random = new Random(Seed);
            Plugin.Log.LogMessage($"Seed set to {seed}");
        }
        public static void Save()
        {
            new SeedSaveData(Seed).Save();
        }

        public static void LoadData(SeedSaveData data)
        {
            SetSeed(data.Seed);
        }
    }
}
