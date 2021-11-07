using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  ///<summary>Filters zones based on given biomes.</summary>
  public class BiomeFilterer : ZoneFilterer {

    private bool IncludeEdges = true;
    private Heightmap.Biome[] Biomes;
    public BiomeFilterer(string[] biomes, bool includeEdges) {
      Biomes = GetBiomes(biomes);
      IncludeEdges = includeEdges;

    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      if (Biomes.Length == 0) return zones;
      var amount = zones.Length;
      zones = FilterByBiomes(zones, Biomes, IncludeEdges);
      var filtered = amount - zones.Length;
      if (filtered > 0) messages.Add(filtered + " filtered by biome");
      return zones;
    }

    private static Vector2i[] FilterByBiomes(Vector2i[] zones, Heightmap.Biome[] biomes, bool edges) {
      if (biomes.Length == 0) return zones;
      var zoneSystem = ZoneSystem.instance;
      var halfZone = zoneSystem.m_zoneSize / 2.0f;
      return zones.Where(zone => {
        var center = zoneSystem.GetZonePos(zone);
        var corner1 = center + new Vector3(halfZone, 0, halfZone);
        var corner2 = center + new Vector3(-halfZone, 0, halfZone);
        var corner3 = center + new Vector3(halfZone, 0, -halfZone);
        var corner4 = center + new Vector3(-halfZone, 0, -halfZone);
        var biome1 = WorldGenerator.instance.GetBiome(corner1);
        var biome2 = WorldGenerator.instance.GetBiome(corner2);
        var biome3 = WorldGenerator.instance.GetBiome(corner3);
        var biome4 = WorldGenerator.instance.GetBiome(corner4);
        if (!edges && (biome1 != biome2 || biome1 != biome3 || biome1 != biome4)) return false;
        return biomes.Any(biome => biome == biome1 || biome == biome2 || biome == biome3 || biome == biome4);
      }).ToArray();
    }
    /// <summary>Converts biome names to biomes.</summary>
    private static Heightmap.Biome[] GetBiomes(string[] biomes) =>
      biomes.Select(GetBiome).Where(biome => biome != Heightmap.Biome.None).ToArray();
    /// <summary>Converts a biome name to a biome.</summary>
    private static Heightmap.Biome GetBiome(string name) {
      name = Helper.Normalize(name);
      if (name.StartsWith("a")) {
        return Heightmap.Biome.AshLands;
      }

      if (name.StartsWith("b")) {
        return Heightmap.Biome.BlackForest;
      }
      if (name.StartsWith("d")) {
        return Heightmap.Biome.DeepNorth;
      }
      if (name.StartsWith("me")) {
        return Heightmap.Biome.Meadows;
      }
      if (name.StartsWith("me")) {
        return Heightmap.Biome.Meadows;
      }
      if (name.StartsWith("mo")) {
        return Heightmap.Biome.Mountain;
      }
      if (name.StartsWith("o")) {
        return Heightmap.Biome.Ocean;
      }

      if (name.StartsWith("p")) {
        return Heightmap.Biome.Plains;
      }

      if (name.StartsWith("s")) {
        return Heightmap.Biome.Swamp;
      }
      return Heightmap.Biome.None;
    }
  }
}