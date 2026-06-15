using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LethalBotsNavMeshProject
{
    // For more info on custom configs, see https://lethal.wiki/dev/intermediate/custom-configs
    // Csync https://lethal.wiki/dev/apis/csync/usage-guide

    /// <summary>
    /// Config class, manage parameters editable by the player (irl)
    /// </summary>
    public class Config : SyncedConfig2<Config>
    {
        private const string ConfigSection = "Lethal Bots NavMesh Project";
        private const string ConfigDebug = "Debug";

        [SyncedEntryField] public SyncedEntry<bool> EnableExperimentationNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableAssuranceNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableVowNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableOffenceNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableAdamanceNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableEmbrionNav;
        [SyncedEntryField] public SyncedEntry<bool> EnableArtificeNav;
        public ConfigEntry<bool> EnableDebugLog;

        public Config(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_GUID)
        {
            cfg.SaveOnConfigSet = false;

            EnableExperimentationNav = cfg.BindSyncedEntry(ConfigSection,
                                            "Enable Experimentation Nav Improvements",
                                            defaultVal: true,
                                            "If you are using a modified version of Experimentation, you may want to disable these improvements!");

            EnableAssuranceNav = cfg.BindSyncedEntry(ConfigSection,
                                                    "Enable Assurance Nav Improvements",
                                                    defaultVal: true,
                                                    "If you are using a modified version of Assurance, you may want to disable these improvements!");

            EnableVowNav = cfg.BindSyncedEntry(ConfigSection,
                                                "Enable Vow Nav Improvements",
                                                defaultVal: true,
                                                "If you are using a modified version of Vow, you may want to disable these improvements!");

            EnableOffenceNav = cfg.BindSyncedEntry(ConfigSection,
                                                    "Enable Offence Nav Improvements",
                                                    defaultVal: true,
                                                    "If you are using a modified version of Offence, you may want to disable these improvements!");

            EnableAdamanceNav = cfg.BindSyncedEntry(ConfigSection,
                                                    "Enable Adamance Nav Improvements",
                                                    defaultVal: true,
                                                    "If you are using a modified version of Adamance, you may want to disable these improvements!");

            EnableEmbrionNav = cfg.BindSyncedEntry(ConfigSection,
                                                    "Enable Embrion Nav Improvements",
                                                    defaultVal: true,
                                                    "If you are using a modified version of Embrion, you may want to disable these improvements!");

            EnableArtificeNav = cfg.BindSyncedEntry(ConfigSection,
                                                    "Enable Artifice Nav Improvements",
                                                    defaultVal: true,
                                                    "If you are using a modified version of Artifice, you may want to disable these improvements!");

            EnableDebugLog = cfg.Bind(ConfigDebug,
                                      "EnableDebugLog  (Client only)",
                                      defaultValue: true,
                                      "Enable the debug logs used for this mod.");

            ClearUnusedEntries(cfg);
            cfg.SaveOnConfigSet = true;
        }

        private void ClearUnusedEntries(ConfigFile cfg)
        {
            // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
            PropertyInfo orphanedEntriesProp = cfg.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg, null);
            orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
            cfg.Save(); // Save the config file to save these changes
        }
    }
}
