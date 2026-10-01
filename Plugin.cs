using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalBots.Constants;
using LethalBotsNavMeshProject.Editor;
using LethalBotsNavMeshProject.MoonNavMeshes;
using NavMeshLib;
using System;
using System.Collections.Generic;
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
        public const string PLUGIN_VERSION = "2.0.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalBots.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string BUNDLE_NAME = "lethalbotsnavmesh";
        internal static string DirectoryName = null!;
        internal static new ManualLogSource Logger = null!;
        internal static new Config Config = null!;
        private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            DirectoryName = Path.GetDirectoryName(Info.Location);

            Logger = base.Logger;
            Config = new Config(base.Config);

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

            try
            {
                _harmony.PatchAll(typeof(RoundManagerPatch));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to patch with error {ex}");
            }

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

            // Check if the current level has a valid navmesh prefab
            string planetName = instanceSOR.currentLevel.PlanetName;
            string sceneName = scene.name;
            string levelName = $"{planetName}.{sceneName}";
            AssetBundle? customAssets = null;
            GameObject? navMeshPrefab = null;
            if (!NavMeshPrefabManager.IsValidLevel(levelName, out bool prefabEnabled) 
                || !prefabEnabled)
            {
                // Check if the moon is disabled or a custom moon or another mod added a NavMeshPrefab for this moon
                if (!prefabEnabled 
                    || !NavMeshPrefabManager.NavMeshAssetPaths.TryGetValue(levelName, out var navMeshInfo)
                    || !navMeshInfo.IsPrefabEnabled)
                {
                    return;
                }

                // Load mod assets from Unity
                Plugin.LogInfo($"Attempting to load custom NavMesh for {levelName} with path {navMeshInfo.filePath}");
                customAssets = AssetBundle.LoadFromFile(navMeshInfo.filePath);
                if (customAssets == null)
                {
                    Plugin.LogError($"Failed to load custom assets with path {navMeshInfo.filePath}. A!");
                    return;
                }

                // Grab the prefab from the asset bundle
                // NEEDTOVALIDATE: Should we just load the first object we find
                GameObject prefab = customAssets.LoadAsset<GameObject>(navMeshInfo.assetName);
                if (prefab == null)
                {
                    Plugin.LogError($"Failed to load custom assets with asset name {navMeshInfo.assetName}. B!");
                    customAssets.Unload(true);
                    return;
                }

                CustomBotNavMeshInfo newNavInfo = prefab.GetComponent<CustomBotNavMeshInfo>();
                if (newNavInfo == null)
                {
                    Plugin.LogError($"Failed to find CustomBotNavMeshInfo with asset name {navMeshInfo.assetName}. C!");
                    customAssets.Unload(true);
                    return;
                }

                // Since this was dynamically loaded, use the dynamic class
                navMeshPrefab = newNavInfo.GetBotNavMeshRoot();
                if (navMeshPrefab == null)
                {
                    Plugin.LogError("Failed to load custom assets. Root Object was null. D!");
                    customAssets.Unload(true);
                    return;
                }

                Plugin.LogInfo("Assets successfully loaded!");
            }

            try
            {
                // Log what we are doing
                Plugin.LogInfo($"Instantiating NavMesh prefab for moon: {planetName} with scene name {sceneName}");
                navMeshPrefab = navMeshPrefab ?? NavMeshPrefabManager.GetPrefabForLevel(levelName);
                if (navMeshPrefab == null)
                {
                    Plugin.LogError($"Failed to get navmesh prefab for level with scene name {sceneName}");
                    return;
                }

                // Find the Environment object, this handles the NavMesh for moons by default.
                Transform? parentTransform = null;
                GameObject? environment = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh") ?? GameObject.Find("Environment");
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

                // Do the bot only navmesh stuff
                Transform? botNavMeshRoot = newNavMesh.transform.Find("LethalBotNavMesh");
                if (botNavMeshRoot != null)
                {
                    // Log what we are doing
                    Plugin.LogInfo("Loading Bot Specific NavMesh!");

                    // Build custom NavMeshSurfaces.
                    NavMeshSurface[] navMeshSurfaces = botNavMeshRoot.GetComponentsInChildren<NavMeshSurface>(includeInactive: true);
                    foreach (NavMeshSurface navMeshSurface in navMeshSurfaces)
                    {
                        // Set the area to Lethal Bots only
                        navMeshSurface.defaultArea = Const.LETHAL_BOT_ONLY_NAVAREA;

                        // Actually build the mesh
                        navMeshSurface.BuildNavMeshAsync();
                    }

                    // Now, we need to update the area mask of the NavMeshLink and OffMeshLink components to be bot only.
                    NavMeshLink[] navMeshLinks = botNavMeshRoot.GetComponentsInChildren<NavMeshLink>(includeInactive: true);
                    foreach (NavMeshLink navMeshLink in navMeshLinks)
                    {
                        navMeshLink.area = Const.LETHAL_BOT_ONLY_NAVAREA;
                        navMeshLink.UpdateLink();
                    }

                    // Until Zeekerss stops using the obsolete OffMeshLink component, we need to disable the warning for it.
                    #pragma warning disable CS0618 // Type or member is obsolete
                    OffMeshLink[] offMeshLinks = botNavMeshRoot.GetComponentsInChildren<OffMeshLink>(includeInactive: true);
                    foreach (OffMeshLink offMeshLink in offMeshLinks)
                    {
                        offMeshLink.area = Const.LETHAL_BOT_ONLY_NAVAREA;
                        offMeshLink.UpdatePositions();
                    }
                    #pragma warning restore CS0618 // Type or member is obsolete
                }

                // Cruiser stuff for the bot
                Transform? cruiserRoot = newNavMesh.transform.Find("LethalBotCruiserNavMesh");
                if (cruiserRoot != null)
                {
                    // Log what we are doing
                    Plugin.LogInfo("Loading Bot Cruiser Specific NavMesh!");

                    // Update NavMeshModifier Volumes
                    NavMeshModifierVolume[] navMeshModifierVolumes = cruiserRoot.GetComponentsInChildren<NavMeshModifierVolume>();
                    foreach (NavMeshModifierVolume navMeshModifier in navMeshModifierVolumes)
                    {
                        // Make sure only the Cruiser is affected by this
                        navMeshModifier.m_AffectedAgents = new List<int>(new int[1] { Const.LETHAL_BOT_CRUISER_NAV_SETTINGS_ID }); // This is how Unity does it.........
                    }

                    // Build custom NavMeshSurfaces.
                    NavMeshSurface[] navMeshSurfaces = cruiserRoot.GetComponentsInChildren<NavMeshSurface>(includeInactive: true);
                    foreach (NavMeshSurface navMeshSurface in navMeshSurfaces)
                    {
                        // Set the area to Lethal Bots only
                        navMeshSurface.agentTypeID = Const.LETHAL_BOT_CRUISER_NAV_SETTINGS_ID;

                        // Actually build the mesh
                        navMeshSurface.BuildNavMeshAsync();
                    }

                    // Now, we need to update the area mask of the NavMeshLink and OffMeshLink components to be bot only.
                    NavMeshLink[] navMeshLinks = cruiserRoot.GetComponentsInChildren<NavMeshLink>(includeInactive: true);
                    foreach (NavMeshLink navMeshLink in navMeshLinks)
                    {
                        navMeshLink.agentTypeID = Const.LETHAL_BOT_CRUISER_NAV_SETTINGS_ID;
                        navMeshLink.UpdateLink();
                    }
                }

                // Support for older stuff prefabs
                if (botNavMeshRoot == null && cruiserRoot == null)
                {
                    // Log about old prefab
                    Plugin.LogWarning($"Old NavMesh prefab is used for {levelName}. \n Ask the mod author to update as the old versions will be removed in a later update.");

                    // Build custom NavMeshSurfaces.
                    NavMeshSurface[] navMeshSurfaces = newNavMesh.GetComponentsInChildren<NavMeshSurface>(includeInactive: true);
                    foreach (NavMeshSurface navMeshSurface in navMeshSurfaces)
                    {
                        // Set the area to Lethal Bots only
                        navMeshSurface.defaultArea = Const.LETHAL_BOT_ONLY_NAVAREA;

                        // Actually build the mesh
                        navMeshSurface.BuildNavMeshAsync();
                    }

                    // Now, we need to update the area mask of the NavMeshLink and OffMeshLink components to be bot only.
                    NavMeshLink[] navMeshLinks = newNavMesh.GetComponentsInChildren<NavMeshLink>(includeInactive: true);
                    foreach (NavMeshLink navMeshLink in navMeshLinks)
                    {
                        navMeshLink.area = Const.LETHAL_BOT_ONLY_NAVAREA;
                        navMeshLink.UpdateLink();
                    }

                    // Until Zeekerss stops using the obsolete OffMeshLink component, we need to disable the warning for it.
                    #pragma warning disable CS0618 // Type or member is obsolete
                    OffMeshLink[] offMeshLinks = newNavMesh.GetComponentsInChildren<OffMeshLink>(includeInactive: true);
                    foreach (OffMeshLink offMeshLink in offMeshLinks)
                    {
                        offMeshLink.area = Const.LETHAL_BOT_ONLY_NAVAREA;
                        offMeshLink.UpdatePositions();
                    }
                    #pragma warning restore CS0618 // Type or member is obsolete
                }
            }
            catch (Exception exception)
            {
                Plugin.LogError($"Exception occurred: {exception}");
            }
            finally
            {
                // Cleanup the old assets
                if (customAssets != null)
                {
                    customAssets.Unload(true);
                    Plugin.LogInfo("Unloaded now unneeded assets.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogDebug(string debugLog)
        {
            if (Plugin.Config.EnableDebugLog.Value)
            {
                Logger.LogDebug(debugLog);
            }
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
