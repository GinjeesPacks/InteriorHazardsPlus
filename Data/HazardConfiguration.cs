#nullable enable
using BepInEx.Configuration;

namespace InteriorHazardsPlus.Data
{
    public class HazardConfiguration
    {
        public bool Enabled { get; set; }
        public int SpawnCount { get; set; }
        public ConfigEntry<bool> EnableConfig { get; set; } = null!;

        public HazardConfiguration(bool enabled, int spawnCount)
        {
            Enabled = enabled;
            SpawnCount = spawnCount;
        }

        public int GetSpawnCount()
        {
            return SpawnCount;
        }
    }
}
