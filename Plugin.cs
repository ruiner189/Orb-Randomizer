using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace OrbRandomizer
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {

        public const String GUID = "com.ruiner.orbrandomizer";
        public const String Name = "Orb Randomizer";
        public const String Version = "1.0.0";

        private Harmony _harmony;
        public static ManualLogSource Log;
        public static ConfigFile ConfigFile;


        private static ConfigEntry<RandomizerType> _randomizerType;
        private static ConfigEntry<bool> _includeOrboros;
        private static ConfigEntry<bool> _includeEgg;
        private static ConfigEntry<bool> _includeMirror;
        private static ConfigEntry<bool> _randomizeStartingDeck;
        private static ConfigEntry<bool> _randomizeCruciballStone;
        private static ConfigEntry<bool> _postSpoilerLog;
        public static RandomizerType RandomizerType => _randomizerType.Value;
        public static bool IncludeOrboros => _includeOrboros.Value;
        public static bool IncludeEgg => _includeEgg.Value;
        public static bool IncludeMirror => _includeMirror.Value;
        public static bool RandomizeStartingDeck => _randomizeStartingDeck.Value;
        public static bool RandomizeCruciballStone => _randomizeCruciballStone.Value;
        public static bool PostSpoilerLog => _postSpoilerLog.Value;

        private void Awake()
        {
            Log = Logger;
            LoadConfig();
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();
        }

        private void LoadConfig()
        {
            ConfigFile = Config;
            _randomizerType = Config.Bind<RandomizerType>("General","RandomizerType", RandomizerType.LEVEL, "The upgrade path for the randomizer.");
            _randomizeStartingDeck = Config.Bind<bool>("General", "RandomizeStartingDeck", false, "Start the game with 4 random orbs.");
            _randomizeCruciballStone = Config.Bind<bool>("General", "RandomizeCruciballStone", false, "Randomizes what the cruciball gives you instead of a stone.");
            _postSpoilerLog = Config.Bind<bool>("General", "PostSpoilerLog", false, "Posts spoiler log into console.");
            _includeOrboros = Config.Bind<bool>("Event Orbs", "IncludeOrboros", true, "Should include orboros in randomizer. Does not change event.");
            _includeEgg = Config.Bind<bool>("Event Orbs", "IncludeEgg", false, "Should include egg in randomizer. Does not change event. Will cause a random orb to not be upgradable if set to LEVEL.");
            _includeMirror = Config.Bind<bool>("Event Orbs", "IncludeMirror", false, "Should include mirror in randomizer. Does not change event. Will cause a random orb to not be upgradeable if set to LEVEL.");
        }
    }
}

