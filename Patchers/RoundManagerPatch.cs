using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using InteriorHazardsPlus.Data;
using BepInEx.Configuration;

namespace InteriorHazardsPlus.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch("SpawnMapObjects")]
        [HarmonyPrefix]
        private static bool Prefix(RoundManager __instance)
        {
            try
            {
                if (!Plugin.Instance.EnableIncreasedSpawns.Value ||
                    __instance.currentLevel.spawnableMapObjects.Length == 0)
                    return true;

                string currentMoon = __instance.currentLevel.name;
                if (!string.IsNullOrEmpty(currentMoon))
                {
                    currentMoon = currentMoon.Replace("level", "", StringComparison.OrdinalIgnoreCase);
                    currentMoon = char.ToUpper(currentMoon[0]) + currentMoon.Substring(1).ToLower();
                }

                // Check if this is a new moon
                if (!Plugin.Instance.HazardSettingsDictionary.ContainsKey(currentMoon) &&
                    !Plugin.Instance.vanillaMoons.ContainsValue(currentMoon))
                {
                    Plugin.LogInfo($"Detected new moon: {currentMoon}. Adding to Custom Moons...");

                    // Update the Custom Moons string
                    string currentCustomMoons = Plugin.Instance.CustomMoons.Value;
                    List<string> customMoonsList = string.IsNullOrEmpty(currentCustomMoons)
                        ? new List<string>()
                        : currentCustomMoons.Split(',').ToList();

                    if (!customMoonsList.Contains(currentMoon))
                    {
                        customMoonsList.Add(currentMoon);
                        Plugin.Instance.CustomMoons.Value = string.Join(",", customMoonsList);
                    }

                    // Add settings for the new moon
                    var settingsDict = new Dictionary<string, Plugin.HazardSettings>();

                    foreach (string hazardType in new[] { "Landmine", "TurretContainer", "SpikeRoofTrap" })
                    {
                        string displayName = hazardType switch
                        {
                            "TurretContainer" => "Turret",
                            "SpikeRoofTrap" => "SpikeTrap",
                            _ => hazardType
                        };

                        var hs = new Plugin.HazardSettings
                        {
                            Enabled = Plugin.Instance.Config.Bind($"13. CUSTOM MOONS - {currentMoon}",
                                $"{displayName}_Enabled",
                                true,
                                $"Enable {displayName} spawns on {currentMoon}"),
                            SpawnCount = Plugin.Instance.Config.Bind($"13. CUSTOM MOONS - {currentMoon}",
                                $"{displayName}_SpawnCount",
                                hazardType switch
                                {
                                    "Landmine" => 20,
                                    "TurretContainer" => 12,
                                    _ => 6
                                },
                                new ConfigDescription(
                                    $"Number of {displayName} to spawn on {currentMoon}",
                                    new AcceptableValueRange<int>(0,
                                        hazardType switch
                                        {
                                            "Landmine" => 50,
                                            "TurretContainer" => 20,
                                            "SpikeRoofTrap" => 30,
                                            _ => 15
                                        })))
                        };

                        settingsDict.Add(hazardType, hs);
                    }
                    Plugin.Instance.HazardSettingsDictionary.Add(currentMoon, settingsDict);
                    Plugin.LogInfo($"Successfully added {currentMoon} to Custom Moons");
                }

                foreach (var hazardKvp in LCUtils.HAZARD_MAP)
                {
                    string hazardType = hazardKvp.Key;

                    if (!Plugin.Instance.HazardSettingsDictionary.TryGetValue(currentMoon, out var settings))
                        continue;

                    if (!settings.TryGetValue(hazardType, out var hazardSettings))
                        continue;

                    if (!hazardSettings.Enabled.Value)
                        continue;

                    int spawnCount = hazardSettings.SpawnCount.Value;

                    if (spawnCount <= 0)
                        continue;

                    var spawnableObject = __instance.currentLevel.spawnableMapObjects.FirstOrDefault(x =>
                        x.prefabToSpawn.name.IndexOf(hazardType, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (spawnableObject == null)
                    {
                        SpawnableMapObject tmpSpawnableObject = StartOfRound.Instance.levels.SelectMany(x =>
                            x.spawnableMapObjects).FirstOrDefault(x =>
                            x.prefabToSpawn.name.IndexOf(hazardType, StringComparison.OrdinalIgnoreCase) >= 0);

                        if (tmpSpawnableObject == null)
                            continue;

                        try
                        {
                            var currentObjects = __instance.currentLevel.spawnableMapObjects;
                            var newObjects = new SpawnableMapObject[currentObjects.Length + 1];
                            Array.Copy(currentObjects, newObjects, currentObjects.Length);
                            newObjects[currentObjects.Length] = tmpSpawnableObject;
                            __instance.currentLevel.spawnableMapObjects = newObjects;
                            spawnableObject = newObjects[currentObjects.Length];
                        }
                        catch (Exception ex)
                        {
                            Plugin.LogError($"Error adding spawnable object for {hazardType}: {ex.Message}");
                            continue;
                        }
                    }

                    if (spawnableObject != null)
                    {
                        spawnableObject.numberToSpawn = AnimationCurve.Constant(0, 1, spawnCount);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Error in SpawnMapObjects patch: {ex}");
                return true;
            }
        }

        private static Vector3 GetSpawnPosition(
            RandomMapObject spawnPoint,
            System.Random random,
            List<Vector3> usedPositions,
            float minDistance = 2f)
        {
            const int MAX_ATTEMPTS = 30;
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                Vector3 randomOffset = new Vector3(
                    ((float)random.NextDouble() - 0.5f) * spawnPoint.spawnRange,
                    0f,
                    ((float)random.NextDouble() - 0.5f) * spawnPoint.spawnRange
                );
                Vector3 candidatePos = spawnPoint.transform.position + randomOffset;

                bool tooClose = usedPositions.Any(pos => Vector3.Distance(candidatePos, pos) < minDistance);
                if (!tooClose)
                    return candidatePos;
            }
            return Vector3.zero;
        }
    }
}

