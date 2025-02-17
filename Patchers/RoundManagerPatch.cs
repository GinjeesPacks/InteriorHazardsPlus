using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using InteriorHazardsPlus.Data;
using InteriorHazardsPlus.plugins;


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
    }
}
