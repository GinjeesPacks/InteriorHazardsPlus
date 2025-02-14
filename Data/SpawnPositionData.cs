using UnityEngine;

namespace InteriorHazardsPlus.Data
{
    public struct SpawnPositionData
    {
        public Vector3 CenterPosition { get; }
        public float SpawnRadius { get; }

        public SpawnPositionData(Vector3 centerPosition, float spawnRadius)
        {
            CenterPosition = centerPosition;
            SpawnRadius = spawnRadius;
        }
    }
}
