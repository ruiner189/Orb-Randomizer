using System;
using UnityEngine;

namespace OrbRandomizer
{
    public class SeedSaveData : SaveObjectData
    {
        public const String SEED_SAVE_KEY = "OrbRandomizer";

        public override string Name => SEED_SAVE_KEY;

        public int Seed => _seed;

        public SeedSaveData(int seed) : base(true)
        {
            _seed = seed;
        }

        [SerializeField]
        private int _seed;

    }
}
