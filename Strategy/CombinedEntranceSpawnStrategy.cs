using System.Collections.Generic;
using UnityEngine;
using InteriorHazardsPlus.Data;

namespace InteriorHazardsPlus.Strategy
{
    public enum CombinedSpawnOption
    {
        MainEntranceOnly,
        FireExitsOnly,
        Both
    }

    internal class CombinedEntranceSpawnStrategy : SpawnStrategy
    {
        public CombinedSpawnOption Option { get; private set; }

        private CombinedEntranceSpawnStrategy(CombinedSpawnOption option)
        {
            Option = option;
        }

        public CombinedEntranceSpawnStrategy()
        {
        }

        public static CombinedEntranceSpawnStrategy GetInstance(CombinedSpawnOption option)
        {
            return new CombinedEntranceSpawnStrategy(option);
        }

        public override List<SpawnPositionData> CalculateCenterPositions(
            Vector3 shipLandPosition,
            Vector3 mainEntrancePosition,
            List<Vector3> fireExitPositions,
            float spawnRadiusMultiplier)
        {
            List<SpawnPositionData> centerPositions = new List<SpawnPositionData>();

            switch (Option)
            {
                case CombinedSpawnOption.MainEntranceOnly:
                    centerPositions.Add(CalculateCenterWithSpawnRadius(shipLandPosition, mainEntrancePosition, spawnRadiusMultiplier));
                    break;
                case CombinedSpawnOption.FireExitsOnly:
                    foreach (Vector3 pos in fireExitPositions)
                    {
                        centerPositions.Add(CalculateCenterWithSpawnRadius(shipLandPosition, pos, spawnRadiusMultiplier));
                    }
                    break;
                case CombinedSpawnOption.Both:
                    centerPositions.Add(CalculateCenterWithSpawnRadius(shipLandPosition, mainEntrancePosition, spawnRadiusMultiplier));
                    foreach (Vector3 pos in fireExitPositions)
                    {
                        centerPositions.Add(CalculateCenterWithSpawnRadius(shipLandPosition, pos, spawnRadiusMultiplier));
                    }
                    break;
            }
            return centerPositions;
        }
    }
}
