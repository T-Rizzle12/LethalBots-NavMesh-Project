using BepInEx;
using LethalBotsNavMeshProject.Editor;
using LethalBotsNavMeshProject.MoonNavMeshes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static LethalBotsNavMeshProject.Editor.CustomBotNavMeshInfo;

namespace LethalBotsNavMeshProject
{
    public static class NavMeshPrefabManager
    {
        public const string BUNDLE_EXTENSION = ".lethalbotsnavmesh";

        private const string EXPERIMENTATION_MOON_SCENE_NAME = "41 Experimentation.Level1Experimentation";
        private const string ASSURANCE_MOON_SCENE_NAME = "220 Assurance.Level2Assurance";
        private const string VOW_MOON_SCENE_NAME = "56 Vow.Level3Vow";
        private const string OFFENSE_MOON_SCENE_NAME = "21 Offense.Level7Offense";
        private const string ADAMANCE_MOON_SCENE_NAME = "20 Adamance.Level10Adamance";
        private const string EMBRION_MOON_SCENE_NAME = "5 Embrion.Level11Embrion";
        private const string ARTIFICE_MOON_SCENE_NAME = "68 Artifice.Level9Artifice";
        private const string TITAN_MOON_SCENE_NAME = "8 Titan.Level8Titan";
        private const string REND_MOON_SCENE_NAME = "85 Rend.Level5Rend";
        private const string DINE_MOON_SCENE_NAME = "7 Dine.Level6Dine";

        #region Dictionarys

        public static readonly Dictionary<string, MoonNavMesh> NavMeshPrefabs = new Dictionary<string, MoonNavMesh>();

        public static readonly Dictionary<string, CustomBotNavMeshInfoData> NavMeshAssetPaths = new Dictionary<string, CustomBotNavMeshInfoData>();

        #endregion

        /// <summary>
        /// Helper event that is called before the default NavMeshPrefabs are loaded
        /// </summary>
        public static readonly UnityEvent LoadCustomNavmeshPrefabs = new UnityEvent(); 

        private static GameObject ExperimentationNavPrefab = null!;
        private static GameObject AssuranceNavPrefab = null!;
        private static GameObject VowNavPrefab = null!;
        private static GameObject OffenseNavPrefab = null!;
        private static GameObject AdamanceNavPrefab = null!;
        private static GameObject EmbrionNavPrefab = null!;
        private static GameObject ArtificeNavPrefab = null!;
        private static GameObject TitanNavPrefab = null!;
        private static GameObject RendNavPrefab = null!;
        private static GameObject DineNavPrefab = null!;

        internal static void LoadPrefabs()
        {
            // Load the nav mesh prefabs from the asset bundle
            if (!ArePrefabsLoaded())
            {
                // Load mod assets from Unity
                AssetBundle modAssets = AssetBundle.LoadFromFile(Path.Combine(Plugin.DirectoryName, Plugin.BUNDLE_NAME));
                if (modAssets == null)
                {
                    Plugin.LogFatal("Failed to load custom assets.");
                    return;
                }

                ExperimentationNavPrefab = modAssets.LoadAsset<GameObject>("ExperimentationNavMesh");
                AssuranceNavPrefab = modAssets.LoadAsset<GameObject>("AssuranceNavMesh");
                VowNavPrefab = modAssets.LoadAsset<GameObject>("VowNavMesh");
                OffenseNavPrefab = modAssets.LoadAsset<GameObject>("OffenseNavMesh");
                AdamanceNavPrefab = modAssets.LoadAsset<GameObject>("AdamanceNavMesh");
                EmbrionNavPrefab = modAssets.LoadAsset<GameObject>("EmbrionNavMesh");
                ArtificeNavPrefab = modAssets.LoadAsset<GameObject>("ArtificeNavMesh");
                TitanNavPrefab = modAssets.LoadAsset<GameObject>("TitanNavMesh");
                RendNavPrefab = modAssets.LoadAsset<GameObject>("RendNavMesh");
                DineNavPrefab = modAssets.LoadAsset<GameObject>("DineNavMesh");
                modAssets.Unload(false);
            }

            // Clear all entries from the dictionary before adding new ones
            NavMeshPrefabs.Clear();

            // Call custom prefab hook
            LoadCustomNavmeshPrefabs.Invoke();

            // Create MoonNavMesh instances and add them to the dictionary
            NavMeshPrefabs.TryAdd(EXPERIMENTATION_MOON_SCENE_NAME, new ExperimentationNavMesh(ExperimentationNavPrefab));
            NavMeshPrefabs.TryAdd(ASSURANCE_MOON_SCENE_NAME, new AssuranceNavMesh(AssuranceNavPrefab));
            NavMeshPrefabs.TryAdd(VOW_MOON_SCENE_NAME, new VowNavMesh(VowNavPrefab));
            NavMeshPrefabs.TryAdd(OFFENSE_MOON_SCENE_NAME, new OffenceNavMesh(OffenseNavPrefab));
            NavMeshPrefabs.TryAdd(ADAMANCE_MOON_SCENE_NAME, new AdamanceNavMesh(AdamanceNavPrefab));
            NavMeshPrefabs.TryAdd(EMBRION_MOON_SCENE_NAME, new EmbrionNavMesh(EmbrionNavPrefab));
            NavMeshPrefabs.TryAdd(ARTIFICE_MOON_SCENE_NAME, new ArtificeNavMesh(ArtificeNavPrefab));
            NavMeshPrefabs.TryAdd(TITAN_MOON_SCENE_NAME, new TitanNavMesh(TitanNavPrefab));
            NavMeshPrefabs.TryAdd(REND_MOON_SCENE_NAME, new RendNavMesh(RendNavPrefab));
            NavMeshPrefabs.TryAdd(DINE_MOON_SCENE_NAME, new DineNavMesh(DineNavPrefab));

            // Load mod added instances
            LoadCustomNavMeshPrefabs();
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
        public static bool IsValidLevel(string levelName, out bool prefabEnabled)
        {
            if (!NavMeshPrefabs.TryGetValue(levelName, out MoonNavMesh moonNavMesh))
            {
                prefabEnabled = true; // HACKHACK: Let the rest of the code to attempt to load a bundle from NavMeshAssetPaths
                return false;
            }
            prefabEnabled = moonNavMesh.IsPrefabEnabled();
            return true;
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
                   TitanNavPrefab != null &&
                   RendNavPrefab != null &&
                   DineNavPrefab != null;
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

        /// <summary>
        /// Loads custom NavMeshPrefabs from custom moons!
        /// </summary>
        internal static async void LoadCustomNavMeshPrefabs()
        {
            // Create new List
            List<string> foundBundles = new List<string>();

            // Move to a worker thread
            await Task.Run(async () =>
            {
                // Find NavMeshPrefabs from other mods
                // Load all paths
                string pluginDir = Paths.PluginPath;
                Plugin.LogDebug($"Searching for bot NavMeshes in: {pluginDir}");
                foreach (string file in Directory.EnumerateFiles(pluginDir, $"*{BUNDLE_EXTENSION}", SearchOption.AllDirectories))
                {
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        Plugin.LogDebug($"Found {fileName} with path {file}");
                        foundBundles.Add(file);
                    }
                }
            });

            // Can't check Unity asset bundles on a worker thread, so we have to do it here
            NavMeshAssetPaths.Clear();
            for (int i = 0; i < foundBundles.Count; i++)
            {
                // Get the asset path and level name
                string assetPath = foundBundles[i];

                // Check if the asset bundle is valid
                AssetBundle? modAssets = AssetBundle.LoadFromFile(assetPath);
                if (modAssets == null)
                {
                    Plugin.LogWarning($"Failed to load custom assets from {assetPath}. Skipping!");
                    continue;
                }

                // Load all CustomBotNavMeshInfo assets from the asset bundle
                // We lazy load the custom NavMeshPrefabs later, so we only need to get the level names here
                GameObject[] prefabs = modAssets.LoadAllAssets<GameObject>();
                List<CustomBotNavMeshInfo> assets = new List<CustomBotNavMeshInfo>();
                foreach (var prefab in prefabs)
                {
                    var info = prefab.GetComponent<CustomBotNavMeshInfo>();
                    if (info != null)
                    {
                        assets.Add(info);
                    }
                }

                // Keep track of used asset names to avoid duplicates
                HashSet<string> usedAssetNames = new HashSet<string>();
                if (assets == null || assets.Count == 0)
                {
                    Plugin.LogWarning($"Failed to find CustomBotNavMeshInfo in {assetPath}. Is it at the root of the prefab?");
                    modAssets.Unload(true);
                    continue;
                }
                for (int j = 0; j < assets.Count; j++)
                {
                    CustomBotNavMeshInfo? info = assets[j];
                    if (info != null)
                    {
                        string levelName = info.GetLevelName();
                        string assetName = info.name;
                        if (NavMeshAssetPaths.ContainsKey(levelName))
                        {
                            Plugin.LogWarning($"Duplicate NavMesh asset found for level with scene name: {levelName}. Overwriting!");
                        }
                        if (!usedAssetNames.Add(assetName))
                        {
                            Plugin.LogError($"Asset with name {assetName} is already loaded for a different level in bundle {assetPath}. Overwriting! THIS WILL CAUSE ISSUES!");
                        }
                        NavMeshAssetPaths[levelName] = new CustomBotNavMeshInfoData() { filePath = assetPath, assetName = assetName };
                    }
                }

                // Unload the asset bundle to free up memory, we store the file path and
                // asset name in the dictionary so we can load it later when needed
                modAssets.Unload(true);
            }
        }
    }
}
