using LethalBotsNavMeshProject.MoonNavMeshes;
using System.Collections.Generic;
using UnityEngine;

namespace LethalBotsNavMeshProject
{
    public static class NavMeshPrefabManager
    {
        private const string EXPERIMENTATION_MOON_SCENE_NAME = "41 Experimentation.Level1Experimentation";
        private const string ASSURANCE_MOON_SCENE_NAME = "220 Assurance.Level2Assurance";
        private const string VOW_MOON_SCENE_NAME = "56 Vow.Level3Vow";
        private const string OFFENSE_MOON_SCENE_NAME = "21 Offense.Level7Offense";
        private const string ADAMANCE_MOON_SCENE_NAME = "20 Adamance.Level10Adamance";
        private const string EMBRION_MOON_SCENE_NAME = "5 Embrion.Level11Embrion";
        private const string ARTIFICE_MOON_SCENE_NAME = "68 Artifice.Level9Artifice";
        private const string TITAN_MOON_SCENE_NAME = "8 Titan.Level8Titan";
        public static readonly Dictionary<string, MoonNavMesh> NavMeshPrefabs = new Dictionary<string, MoonNavMesh>();

        private static GameObject ExperimentationNavPrefab = null!;
        private static GameObject AssuranceNavPrefab = null!;
        private static GameObject VowNavPrefab = null!;
        private static GameObject OffenseNavPrefab = null!;
        private static GameObject AdamanceNavPrefab = null!;
        private static GameObject EmbrionNavPrefab = null!;
        private static GameObject ArtificeNavPrefab = null!;
        private static GameObject TitanNavPrefab = null!;

        internal static void LoadPrefabs()
        {
            // Load the nav mesh prefabs from the asset bundle
            if (!ArePrefabsLoaded())
            {
                ExperimentationNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("ExperimentationNavMesh");
                AssuranceNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("AssuranceNavMesh");
                VowNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("VowNavMesh");
                OffenseNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("OffenseNavMesh");
                AdamanceNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("AdamanceNavMesh");
                EmbrionNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("EmbrionNavMesh");
                ArtificeNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("ArtificeNavMesh");
                TitanNavPrefab = Plugin.ModAssets.LoadAsset<GameObject>("TitanNavMesh");
            }

            // Clear all entries from the dictionary before adding new ones
            NavMeshPrefabs.Clear();

            // Create MoonNavMesh instances and add them to the dictionary
            NavMeshPrefabs.TryAdd(EXPERIMENTATION_MOON_SCENE_NAME, new ExperimentationNavMesh(ExperimentationNavPrefab));
            NavMeshPrefabs.TryAdd(ASSURANCE_MOON_SCENE_NAME, new AssuranceNavMesh(AssuranceNavPrefab));
            NavMeshPrefabs.TryAdd(VOW_MOON_SCENE_NAME, new VowNavMesh(VowNavPrefab));
            NavMeshPrefabs.TryAdd(OFFENSE_MOON_SCENE_NAME, new OffenceNavMesh(OffenseNavPrefab));
            NavMeshPrefabs.TryAdd(ADAMANCE_MOON_SCENE_NAME, new AdamanceNavMesh(AdamanceNavPrefab));
            NavMeshPrefabs.TryAdd(EMBRION_MOON_SCENE_NAME, new EmbrionNavMesh(EmbrionNavPrefab));
            NavMeshPrefabs.TryAdd(ARTIFICE_MOON_SCENE_NAME, new ArtificeNavMesh(ArtificeNavPrefab));
            NavMeshPrefabs.TryAdd(TITAN_MOON_SCENE_NAME, new TitanNavMesh(TitanNavPrefab));
        }

        /// <summary>
        /// Gets the NavPrefab for the given <paramref name="levelName"/>
        /// </summary>
        /// <param name="levelName">This should be <see cref="SelectableLevel.PlanetName"/>.<see cref="SelectableLevel.sceneName"/>.<br/> For example: 41 Experimentation.Level1Experimentation</param>
        /// <returns></returns>
        public static GameObject? GetPrefabForLevel(string levelName)
        {
            if (!NavMeshPrefabs.TryGetValue(levelName, out MoonNavMesh? moonNavMesh))
            {
                Plugin.LogError($"No navmesh prefab found for level with scene name: {levelName}");
                return null;
            }
            return moonNavMesh.GetNavPrefab();
        }

        /// <summary>
        /// Checks if a NavPrefab exists for the given <paramref name="levelName"/>
        /// </summary>
        /// <param name="levelName">This should be <see cref="SelectableLevel.PlanetName"/>.<see cref="SelectableLevel.sceneName"/>.<br/> For example: 41 Experimentation.Level1Experimentation</param>
        /// <returns></returns>
        public static bool IsValidLevel(string levelName)
        {
            return NavMeshPrefabs.TryGetValue(levelName, out MoonNavMesh moonNavMesh) && moonNavMesh.IsPrefabEnabled();
        }

        /// <summary>
        /// Checks if every prefab was successfully loaded
        /// </summary>
        /// <returns></returns>
        public static bool ArePrefabsLoaded()
        {
            return ExperimentationNavPrefab != null &&
                   AssuranceNavPrefab != null &&
                   VowNavPrefab != null &&
                   OffenseNavPrefab != null &&
                   AdamanceNavPrefab != null &&
                   EmbrionNavPrefab != null &&
                   ArtificeNavPrefab != null &&
                   TitanNavPrefab != null;
        }

        /// <summary>
        /// Lists every Nav Prefab in <see cref="NavMeshPrefabs"/>
        /// </summary>
        public static void LogPrefabStatus()
        {
            foreach (var moonNavData in NavMeshPrefabs)
            {
                MoonNavMesh moonNavMesh = moonNavData.Value;
                if (moonNavMesh != null)
                {
                    GameObject? navPrefab = moonNavMesh.GetNavPrefab();
                    if (navPrefab != null)
                    {
                        Plugin.LogInfo($"Navmesh prefab found for level with scene name: {moonNavData.Key}. \n Is Prefab Enabled: {moonNavMesh.IsPrefabEnabled()}");
                        continue;
                    }
                }

                Plugin.LogWarning($"No navmesh prefab found for level with scene name: {moonNavData.Key}");
            }
        }
    }
}
