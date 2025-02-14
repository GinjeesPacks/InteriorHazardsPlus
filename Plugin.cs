#nullable enable
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace InteriorHazardsPlus
{
    [BepInPlugin("GinjeesPacks.InteriorHazardsPlus", "InteriorHazardsPlus", "1.0.0")]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "GinjeesPacks.InteriorHazardsPlus";
        public const string NAME = "InteriorHazardsPlus";
        public const string VERSION = "1.0.0";
        public static Plugin Instance = null!;
        private readonly Harmony harmony = new Harmony(GUID);
        private static ManualLogSource Log = null!;

        public ConfigEntry<bool> EnableIncreasedSpawns = null!;
        public ConfigEntry<string> CustomMoons = null!;
        public Dictionary<string, Dictionary<string, HazardSettings>> HazardSettingsDictionary = new();

        public readonly Dictionary<string, string> vanillaMoons = new Dictionary<string, string>
        {
            {"02. Experimentation", "Experimentation"},
            {"03. Assurance", "Assurance"},
            {"04. Vow", "Vow"},
            {"05. Offense", "Offense"},
            {"06. March", "March"},
            {"07. Adamance", "Adamance"},
            {"08. Rend", "Rend"},
            {"09. Dine", "Dine"},
            {"10. Titan", "Titan"},
            {"11. Artifice", "Artifice"},
            {"12. Embrion", "Embrion"}
        };

        public class HazardSettings
        {
            public ConfigEntry<bool> Enabled = null!;
            public ConfigEntry<int> SpawnCount = null!;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            Log = Logger;
            LoadConfig();

            try
            {
                harmony.PatchAll();
                LogInfo($"{NAME} {VERSION} patches applied successfully!");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply patches: {ex}");
            }
            LogInfo($"{NAME} {VERSION} is loaded!");
        }

        private void LoadConfig()
        {
            EnableIncreasedSpawns = Config.Bind("01. GENERAL",
                "EnableIncreasedSpawns",
                true,
                "Enable increased hazard spawns inside facilities");

            HazardSettingsDictionary = new Dictionary<string, Dictionary<string, HazardSettings>>();

            foreach (var moon in vanillaMoons)
            {
                var settingsDict = new Dictionary<string, HazardSettings>();
                string sectionName = moon.Key;
                string moonName = moon.Value;

                foreach (string hazardType in new[] { "Landmine", "TurretContainer", "SpikeRoofTrap" })
                {
                    string displayName = hazardType switch
                    {
                        "TurretContainer" => "Turret",
                        "SpikeRoofTrap" => "SpikeTrap",
                        _ => hazardType
                    };

                    var hs = new HazardSettings
                    {
                        Enabled = Config.Bind(sectionName,
                            $"{displayName}_Enabled",
                            true,
                            $"Enable {displayName} spawns on {moonName}"),
                        SpawnCount = Config.Bind(sectionName,
                            $"{displayName}_SpawnCount",
                            hazardType switch
                            {
                                "Landmine" => 20,
                                "TurretContainer" => 12,
                                _ => 6
                            },
                            new ConfigDescription(
                                $"Number of {displayName} to spawn on {moonName}",
                                new AcceptableValueRange<int>(0,
                                    hazardType switch
                                    {
                                        "Landmine" => 40,
                                        "TurretContainer" => 20,
                                        "SpikeRoofTrap" => 30,
                                        _ => 15
                                    })))
                    };

                    settingsDict.Add(hazardType, hs);
                }
                HazardSettingsDictionary.Add(moon.Value, settingsDict);
            }

            CustomMoons = Config.Bind("13. CUSTOM MOONS",
                "AdditionalMoons",
                "",
                "Custom moons will be automatically detected from LethalLevelLoader");

            LogInfo($"{NAME} configs applied successfully!");
        }

        public int GetHazardSpawnCount(string hazardType, string currentMoon)
        {
            if (HazardSettingsDictionary.TryGetValue(currentMoon, out var settings) &&
                settings.TryGetValue(hazardType, out var hs))
            {
                return hs.SpawnCount.Value;
            }
            return hazardType switch
            {
                "Landmine" => 20,
                "TurretContainer" => 12,
                _ => 6
            };
        }

        public static void LogInfo(string message) => Log?.LogInfo(message);
        public static void LogError(string message) => Log?.LogError(message);
        public static void LogDebug(string message) => Log?.LogDebug(message);
        public static void LogWarning(string message) => Log?.LogWarning(message);
    }
}

