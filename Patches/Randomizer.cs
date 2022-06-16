using Battle.Enemies;
using HarmonyLib;
using OrbRandomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Serialization;
using UnityEngine;
using Worldmap;

namespace Orb_Randomizer.Patches
{
    public static class Randomizer
    {
        private static List<GameObject> _allOrbs = new List<GameObject>();
        private static List<GameObject> _selectionPool = new List<GameObject>();

        public static void Randomize()
        {
            Plugin.Log.LogMessage("Randomizer start!");
            GetRandomSelectionPool();
            ClearNextLevelPrefab();

            if (Plugin.RandomizerType == RandomizerType.LOOP)
                LoopRandom();
            else if (Plugin.RandomizerType == RandomizerType.LEVEL)
                LevelRandom();

            Plugin.Log.LogMessage("Randomizer complete!");
        }


        public static void GatherOrbs()
        {
            _allOrbs.Clear();
            // We are only gathering from the shared level 1 pool. Any orbs added via scenerios have to be manually added in
            UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll<OrbPool>();

            if (objects.Length == 0)
            {
                Plugin.Log.LogWarning("Could not find orb pool to get orb data");
                return;
            }

            GameObject[] orbs = ((OrbPool)objects[0]).AvailableOrbs;

            foreach (GameObject orb in orbs)
            {
                _allOrbs.Add(orb);

                Attack attack = orb.GetComponent<Attack>();
                while (attack != null)
                {
                    if(attack.NextLevelPrefab != null)
                    {
                        _allOrbs.Add(attack.NextLevelPrefab);
                        attack = attack.NextLevelPrefab.GetComponent<Attack>();
                    } else
                    {
                        attack = null;
                    }
                }
            }

            _allOrbs.AddRange(GatherEventOrbs());

            _allOrbs = _allOrbs.OrderBy(x => {
                Attack attack = x.GetComponent<Attack>();
                return $"{attack.locName}-{attack.Level}";
            }).ToList();
        }

        private static List<GameObject> GatherEventOrbs()
        {
            List<GameObject> list = new List<GameObject>();

            if (Plugin.IncludeOrboros)
            {
                list.Add(LoadOrbPrefab("Orboros-Lvl1"));
                list.Add(LoadOrbPrefab("Orboros-Lvl2"));
                list.Add(LoadOrbPrefab("Orboros-Lvl3"));
            }

            if (Plugin.IncludeEgg)
            {
                list.Add(LoadOrbPrefab("Egg-Lvl1"));
            }

            if (Plugin.IncludeMirror)
            {
                list.Add(LoadOrbPrefab("Mirrorb-Lvl1"));
            }

            return list;
        }

        private static GameObject LoadOrbPrefab(String name)
        {
            return Resources.Load<GameObject>($"Prefabs/Orbs/{name}");
        }

        public static void GetRandomSelectionPool()
        {
            if(_allOrbs.Count == 0)
            {
                GatherOrbs();
            }
            _selectionPool = ShuffleList<GameObject>(_allOrbs);
        }

        private static void ClearNextLevelPrefab()
        {
            foreach(GameObject orb in _allOrbs)
            {
                Attack attack = orb.GetComponent<Attack>();
                if (attack != null) attack.NextLevelPrefab = null;
            }
        }

        private static void LevelRandom()
        {
            int lastCount = -1;
            while(_selectionPool.Count > 0 && lastCount != _selectionPool.Count)
            {
                lastCount = _selectionPool.Count;

                GameObject levelOne = GetObjectOfLevel(1);
                GameObject levelTwo = GetObjectOfLevel(2);
                GameObject levelThree = GetObjectOfLevel(3);

                Attack attackOne = levelOne.GetComponent<Attack>();
                Attack attackTwo = levelTwo.GetComponent<Attack>();

                attackOne.NextLevelPrefab = levelTwo;
                attackTwo.NextLevelPrefab = levelThree;


                if (levelOne != null)
                    _selectionPool.Remove(levelOne);
                if (levelTwo != null)
                    _selectionPool.Remove(levelTwo);
                if (levelThree != null)
                    _selectionPool.Remove(levelThree);

                if(Plugin.PostSpoilerLog)
                    Plugin.Log.LogInfo($"{levelOne.name} => {levelTwo.name} => {levelThree.name}");
            }
        }

        private static void LoopRandom()
        {
            if (_selectionPool.Count == 0) return;

            GameObject firstOrb = _selectionPool[0];
            GameObject currentOrb = firstOrb;

            _selectionPool.RemoveAt(0);
            while (_selectionPool.Count > 0)
            {
                GameObject nextOrb = _selectionPool[0];

                Attack attack = currentOrb.GetComponent<Attack>();
                if(attack != null)
                {
                    attack.NextLevelPrefab = nextOrb;
                }
                if(Plugin.PostSpoilerLog)
                    Plugin.Log.LogInfo($"{currentOrb.name} => {nextOrb.name}");
                currentOrb = nextOrb;

                _selectionPool.RemoveAt(0);
            }

            if(currentOrb != firstOrb)
            {
                Attack attack = currentOrb.GetComponent<Attack>();
                if(attack != null)
                {
                    attack.NextLevelPrefab = firstOrb;
                }

                if (Plugin.PostSpoilerLog)
                    Plugin.Log.LogInfo($"{currentOrb.name} => {firstOrb.name}");
            }
        }

        private static GameObject GetObjectOfLevel(int level)
        {
            return _selectionPool.Find(obj =>
            {

                Attack attack = obj.GetComponent<Attack>();
                if (attack != null)
                {
                    if (attack.Level == level) return true;
                }
                return false;
            });
        }
            

        /**
         * Fisher_Yates_CardDeck_Shuffle Algorithm
         */
        private static List<T> ShuffleList<T>(List<T> list)
        {
            System.Random random = SeedManager.Random;
            List<T> newList = new List<T>(list);

            T pntr;
            int n = newList.Count;
            for (int i = 0; i < n; i++)
            {
                int r = i + (int)(random.NextDouble() * (n - i));
                pntr = newList[r];
                newList[r] = newList[i];
                newList[i] = pntr;
            }

            return newList;
        }


        [HarmonyPatch(typeof(MapController), nameof(MapController.SaveData))]
        public static class SaveSeed
        {
            public static void Prefix()
            {
                SeedManager.Save();
            }
        }

        [HarmonyPatch(typeof(GameInit), nameof(GameInit.Start))]
        public static class StartRandomizer
        {
            [HarmonyPriority(Priority.Last)]
            public static void Prefix(GameInit __instance)
            {
                if (__instance.LoadData.NewGame)
                {
                    SeedManager.SetSeed((int)DateTime.Now.Ticks);
                }
                else
                {
                    SeedSaveData saveData = (SeedSaveData)DataSerializer.Load<SaveObjectData>(SeedSaveData.SEED_SAVE_KEY);
                    if (saveData != null)
                    {
                        SeedManager.SetSeed(saveData.Seed);
                    }
                    else
                    {
                        SeedManager.SetSeed((int)DateTime.Now.Ticks);
                    }
                }
                Randomize();

                System.Random rand = SeedManager.Random;
                List<GameObject> randomOrbs = ShuffleList<GameObject>(_allOrbs);
                randomOrbs.RemoveAll(orb =>
                {
                    Attack attack = orb.GetComponent<Attack>();
                    if (attack.Level != 1) return true;
                    return false;
                });

                if (Plugin.RandomizeStartingDeck)
                {
                    List<GameObject> starterDeck = __instance._initialDeck.Balls;
                    starterDeck.Clear();
                    for (int i = 0; i < 4; i++)
                    {
                        int value = rand.Next(0, randomOrbs.Count);
                        starterDeck.Add(randomOrbs[value]);
                    }
                }

                if (Plugin.RandomizeCruciballStone)
                    __instance._cruciballManager.stonePrefab = randomOrbs[rand.Next(0, randomOrbs.Count)].GetComponent<PachinkoBall>();
            }

        }
    }
}
