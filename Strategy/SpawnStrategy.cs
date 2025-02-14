using System.Collections.Generic;
using UnityEngine;
using InteriorHazardsPlus.Data;


namespace InteriorHazardsPlus.Strategy
{
    public abstract class SpawnStrategy
    {
        public abstract List<SpawnPositionData> CalculateCenterPositions(
            Vector3 shipLandPosition,
            Vector3 mainEntrancePosition,
            List<Vector3> fireExitPositions,
            float spawnRadiusMultiplier);

        protected SpawnPositionData CalculateCenterWithSpawnRadius(
            Vector3 shipLandPosition,
            Vector3 targetPosition,
            float spawnRadiusMultiplier)
        {
            Vector3 centerPos = Vector3.Lerp(shipLandPosition, targetPosition, 0.5f);
            float distance = Vector3.Distance(shipLandPosition, targetPosition);
            float radius = distance * spawnRadiusMultiplier;
            return new SpawnPositionData(centerPos, radius);
        }
    }
}
