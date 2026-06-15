using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LethalBotsNavMeshProject
{
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "T-Rizzle.LethalBotsNavMeshProject";
        public const string PLUGIN_NAME = "LethalBotsNavMeshProject";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalBots.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle ModAssets = null!;
        internal static string DirectoryName = null!;
        internal static new ManualLogSource Logger = null!;
        internal static new Config Config = null!;
        private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            var bundleName = "lethalbotsnavmesh";
            DirectoryName = Path.GetDirectoryName(Info.Location);

            Logger = base.Logger;
            Config = new Config(base.Config);

            // Load mod assets from Unity
            ModAssets = AssetBundle.LoadFromFile(Path.Combine(DirectoryName, bundleName));
            if (ModAssets == null)
            {
                Plugin.LogFatal("Failed to load custom assets.");
                return;
            }

            // Load the nav mesh prefab from the asset bundle
            NavMeshPrefabManager.LoadPrefabs();
            if (!NavMeshPrefabManager.ArePrefabsLoaded())
            {
                Plugin.LogWarning("Failed to load some or all of the nav mesh prefabs from the asset bundle.");
                Plugin.LogWarning("Some levels may not have custom nav meshes.");
            }

            // Log the prefab status
            NavMeshPrefabManager.LogPrefabStatus();

            SceneManager.sceneLoaded += OnSceneLoaded;

            Plugin.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only do this for moons that we have navmesh prefabs for!
            StartOfRound instanceSOR = StartOfRound.Instance;
            if (instanceSOR == null 
                || instanceSOR.currentLevel == null)
            {
                return;
            }

            string levelName = $"{instanceSOR.currentLevel.PlanetName}.{instanceSOR.currentLevel.sceneName}";
            if (!NavMeshPrefabManager.IsValidLevel(levelName))
            {
                return;
            }

            try
            {
                Plugin.LogInfo($"Instantiating NavMesh prefab for moon: {instanceSOR.currentLevel.PlanetName} with scene name {instanceSOR.currentLevel.sceneName}");
                GameObject? navMeshPrefab = NavMeshPrefabManager.GetPrefabForLevel(levelName);
                if (navMeshPrefab == null)
                {
                    Plugin.LogError($"Failed to get navmesh prefab for level with scene name {instanceSOR.currentLevel.sceneName}");
                    return;
                }

                // Find the Environment object, this handles the NavMesh for moons by default.
                Transform? parentTransform = null;
                GameObject? environment = GameObject.Find("Environment");
                if (environment == null)
                {
                    Plugin.LogError("Failed to find Environment object in the scene!");
                    return;
                }

                // Try to find the NavMeshColliders object.
                Transform? navMeshColliders = environment.transform.Find("NavMeshColliders");
                if (navMeshColliders == null)
                {
                    // Fall back to parenting to the Environment object, but log a warning since this is not ideal.
                    // NOTE: This is mostly because while NavMeshColliders has no local offset,
                    // this could change in the future and cause issues with the prefab!
                    Plugin.LogWarning("Failed to find NavMeshColliders! Fallback to parenting to the Environment object.");
                    Plugin.LogError("This may cause the NavFixes to fail to work. Report this to the mod devs!");
                    parentTransform = environment.transform;
                }
                else
                {
                    // If we found the NavMeshColliders object, we will parent the navmesh prefab to it.
                    parentTransform = navMeshColliders;
                }

                // Instantiate the navmesh prefab and parent it to the NavMeshColliders object (or Environment if we failed to find it).
                GameObject? newNavMesh = GameObject.Instantiate(navMeshPrefab, parentTransform);

                // Now, we need to update the area mask of the NavMeshLink and OffMeshLink components to be bot only.
                NavMeshLink[] navMeshLinks = newNavMesh.GetComponentsInChildren<NavMeshLink>(includeInactive: true);
                foreach (NavMeshLink navMeshLink in navMeshLinks)
                {
                    navMeshLink.area = LethalBots.Constants.Const.LETHAL_BOT_ONLY_NAVAREA;
                    navMeshLink.UpdateLink();
                }

                // Until Zeekerss stops using the obsolete OffMeshLink component, we need to disable the warning for it.
                #pragma warning disable CS0618 // Type or member is obsolete
                OffMeshLink[] offMeshLinks = newNavMesh.GetComponentsInChildren<OffMeshLink>(includeInactive: true);
                foreach (OffMeshLink offMeshLink in offMeshLinks)
                {
                    offMeshLink.area = LethalBots.Constants.Const.LETHAL_BOT_ONLY_NAVAREA;
                    offMeshLink.UpdatePositions();
                }
                #pragma warning restore CS0618 // Type or member is obsolete
            }
            catch (Exception exception)
            {
                Plugin.LogError($"Exception occurred: {exception}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogDebug(string debugLog)
        {
            Logger.LogDebug(debugLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogInfo(string infoLog)
        {
            Logger.LogInfo(infoLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning(string warningLog)
        {
            Logger.LogWarning(warningLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(string errorLog)
        {
            Logger.LogError(errorLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogFatal(string errorLog)
        {
            Logger.LogFatal(errorLog);
        }
    }
}
