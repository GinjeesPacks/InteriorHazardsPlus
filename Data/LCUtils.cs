#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace InteriorHazardsPlus.Data
{
    public static class LCUtils
    {
        public static readonly Dictionary<string, string[]> CUSTOM_LAYER_MASK = new Dictionary<string, string[]>()
        {
            { "march", new string[] { "Room" } }
        };

        public static readonly Dictionary<string, HazardType> HAZARD_MAP = new Dictionary<string, HazardType>()
        {
            { "Landmine", HazardType.Landmine },
            { "TurretContainer", HazardType.Turret },
            { "SpikeRoofTrap", HazardType.SpikeRoofTrap }
        };

        public static float RandomNumberInRadius(float radius, System.Random? randomSeed)
        {
            randomSeed ??= new System.Random();
            return (float)((randomSeed.NextDouble() - 0.5) * radius);
        }

        public static (Vector3, Quaternion) GetRandomInteriorPositionAndRotation(
            Vector3 centerPoint,
            float radius = 10f,
            System.Random? randomSeed = null,
            int maxAttempts = 10)
        {
            int interiorLayerMask = LayerMask.GetMask("Room");
            float originalY = centerPoint.y;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    float offsetX = RandomNumberInRadius(radius, randomSeed);
                    float offsetY = RandomNumberInRadius(radius, randomSeed);
                    float offsetZ = RandomNumberInRadius(radius, randomSeed);
                    Vector3 candidate = centerPoint + new Vector3(offsetX, offsetY, offsetZ);
                    candidate.y = originalY;

                    float maxDistance = Vector3.Distance(centerPoint, candidate) + 30f;

                    if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, maxDistance, interiorLayerMask))
                    {
                        RaycastHit hitInfo;
                        if (Physics.Raycast(navHit.position + Vector3.up, Vector3.down, out hitInfo, 50f, interiorLayerMask))
                        {
                            return (hitInfo.point + Vector3.up * 0.1f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogError($"Error finding spawn position: {ex.Message}");
                }
            }
            return (Vector3.zero, Quaternion.identity);
        }
    }
}
