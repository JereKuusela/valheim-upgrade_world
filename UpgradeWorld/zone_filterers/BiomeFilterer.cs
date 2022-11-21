using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Filters zones based on given biomes.</summary>
public class BiomeFilterer : ZoneFilterer
{

  private bool IncludeEdges = true;
  private IEnumerable<Heightmap.Biome> Biomes;
  public BiomeFilterer(IEnumerable<Heightmap.Biome> biomes, bool includeEdges)
  {
    Biomes = biomes;
    IncludeEdges = includeEdges;
  }

  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    if (Biomes.Count() == 0) return zones;
    var amount = zones.Length;
    zones = FilterByBiomes(zones);
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by having wrong biomes");
    return zones;
  }

  private Vector2i[] FilterByBiomes(Vector2i[] zones)
  {
    var zoneSystem = ZoneSystem.instance;
    var halfZone = zoneSystem.m_zoneSize / 2.0f;
    return zones.Where(zone =>
    {
      var center = zoneSystem.GetZonePos(zone);
      var corner1 = center + new Vector3(halfZone, 0, halfZone);
      var corner2 = center + new Vector3(-halfZone, 0, halfZone);
      var corner3 = center + new Vector3(halfZone, 0, -halfZone);
      var corner4 = center + new Vector3(-halfZone, 0, -halfZone);
      var biome1 = WorldGenerator.instance.GetBiome(corner1);
      var biome2 = WorldGenerator.instance.GetBiome(corner2);
      var biome3 = WorldGenerator.instance.GetBiome(corner3);
      var biome4 = WorldGenerator.instance.GetBiome(corner4);
      if (!IncludeEdges && (biome1 != biome2 || biome1 != biome3 || biome1 != biome4)) return false;
      return Biomes.Any(biome => biome == biome1 || biome == biome2 || biome == biome3 || biome == biome4);
    }).ToArray();
  }
}
