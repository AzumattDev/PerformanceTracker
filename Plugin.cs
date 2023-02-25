using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FPSCounter;
using HarmonyLib;
using JetBrains.Annotations;
using PerformanceTracker.Util;
using PerformanceTracker.Util.Helpers;
using UnityEngine;

namespace PerformanceTracker
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class PerformanceTrackerPlugin : BaseUnityPlugin
    {
        internal const string ModName = "PerformanceTracker";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "Azumatt";
        internal const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        internal static PerformanceTrackerPlugin ModContext;
        internal static GameObject pluginGO = null!;
        public static readonly ManualLogSource PerformanceTrackerLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public enum CounterColors
        {
            White,
            Black,
            Outline
        }

        public void Awake()
        {
            ModContext = this;
            pluginGO = gameObject;
            
            _showCounter = config("1 - General", "Toggle counter and reset stats",
                new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift), "Key to enable and disable the plugin.");
            _shown = config("1 - General", "Enable", Toggle.On, "Monitor performance statistics and show them on the screen. When disabled the plugin has no effect on performance.");
            _showPluginStats = config("1 - General", "Enable monitoring plugins", Toggle.On, "Count time each plugin takes every frame to execute. Only detects MonoBehaviour event methods, so results might be lower than expected. Has a small performance penalty.");
            _showUnityMethodStats = config("1 - General", "Show detailed frame stats", Toggle.On, "Show how much time was spent by Unity in each part of the frame, for example how long it took to run all Update methods.");

            try
            {
                var procMem = MemoryInfo.QueryProcessMemStatus();
                var memorystatus = MemoryInfo.QuerySystemMemStatus();
                if (procMem.WorkingSetSize <= 0 || memorystatus.ullTotalPhys <= 0)
                    throw new IOException("Empty data was returned");

                _measureMemory = config("General", "Show memory and GC stats", Toggle.On,
                    "Show memory usage of the process, free available physical memory and garbage collector statistics (if available).");
            }
            catch (Exception ex)
            {
                PerformanceTrackerLogger.LogWarning("Memory statistics are not available - " + ex.Message);
            }

            _position = config("Interface", "Screen position", TextAnchor.LowerRight,
                "Which corner of the screen to display the statistics in.");
            _counterColor = config("Interface", "Color of the text", CounterColors.White,
                "Color of the displayed stats. Outline has a performance hit but it always easy to see.");

            _position.SettingChanged += (sender, args) => UIHelper.UpdateLooks();
            _counterColor.SettingChanged += (sender, args) => UIHelper.UpdateLooks();
            _shown.SettingChanged += (sender, args) =>
            {
                UIHelper.UpdateLooks();
                Helpers.SetCapturingEnabled(_shown.Value == Toggle.On);
            };
            _showPluginStats.SettingChanged += (sender, args) => Helpers.SetCapturingEnabled(_shown.Value == Toggle.On);

            OnEnable();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnEnable()
        {
            Helpers.OnEnable();
        }
        
        private void OnDisable()
        {
            Helpers.OnDisable();
        }

        private void Update()
        {
            if (_showCounter.Value.IsDown())
                _shown.Value = _shown.Value == Toggle.Off ? Toggle.On : Toggle.Off;
        }

        private void OnDestroy()
        {
            Config.Save();
            Helpers.SetCapturingEnabled(false);
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                PerformanceTrackerLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                PerformanceTrackerLogger.LogError($"There was an issue loading your {ConfigFileName}");
                PerformanceTrackerLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        private static ConfigEntry<KeyboardShortcut> _showCounter;
        internal static ConfigEntry<CounterColors> _counterColor;
        internal static ConfigEntry<TextAnchor> _position;
        internal static ConfigEntry<Toggle> _shown;
        internal static ConfigEntry<Toggle> _showPluginStats;
        internal static ConfigEntry<Toggle> _showUnityMethodStats;
        internal static ConfigEntry<Toggle> _measureMemory;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
            //var configEntry = Config.Bind(group, name, value, description);

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description)
        {
            return config(group, name, value, new ConfigDescription(description));
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        #endregion
    }
}