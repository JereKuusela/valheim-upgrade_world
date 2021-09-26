using System;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  /// <summary>First step of the upgrader: Get an array of zones determined by the upgrade command.</summary>
  public static class Filter {
    /// <summary>Filters given zones by biomes.</summary>
    public static Vector2i[] FilterByBiomes(Vector2i[] zones, Heightmap.Biome[] included, Heightmap.Biome[] excluded) {
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
        var isIncluded = included.Count() == 0 || included.Any(biome => biome == biome1 || biome == biome2 || biome == biome3 || biome == biome4);
        var isExcluded = excluded.Any(biome => biome == biome1 || biome == biome2 || biome == biome3 || biome == biome4);
        return isIncluded && !isExcluded;
      }).ToArray();
    }
    /// <summary>Returns zones that are fully within a given range.</summary>
    public static Vector2i[] FilterByRange(Vector2i[] zones, Vector3 position, float min, float max) {
      var zoneSystem = ZoneSystem.instance;
      var halfZone = zoneSystem.m_zoneSize / 2.0f;
      return zones.Where(zone => {
        var center = zoneSystem.GetZonePos(zone);
        var distance = center - position;
        center.y = 0f;
        var corner1 = distance + new Vector3(halfZone, 0, halfZone);
        var corner2 = distance + new Vector3(-halfZone, 0, halfZone);
        var corner3 = distance + new Vector3(halfZone, 0, -halfZone);
        var corner4 = distance + new Vector3(-halfZone, 0, -halfZone);
        var outsideMin = min == 0 || (corner1.magnitude >= min && corner2.magnitude >= min && corner3.magnitude >= min && corner4.magnitude >= min);
        var insideMax = max == 0 || (corner1.magnitude <= max && corner2.magnitude <= max && corner3.magnitude <= max && corner4.magnitude <= max);
        return outsideMin && insideMax;
      }).ToArray();
    }
    /// <summary>Returns only zones that are within a given adjacencty to a given center zone.</summary>
    public static Vector2i[] FilterByAdjacent(Vector2i[] zones, Vector2i centerZone, int adjacent) {
      return zones.Where(zone => {
        var withinX = Math.Abs(centerZone.x - zone.x) <= adjacent;
        var withinY = Math.Abs(centerZone.y - zone.y) <= adjacent;
        return withinX && withinY;
      }).ToArray();
    }

    /// <summary>Returns only zones that are included within a given distance.</summary>
    // From: https://stackoverflow.com/a/402010
    public static Vector2i[] FilterByRadius(Vector2i[] zones, Vector3 position, float radius) {
      var zoneSystem = ZoneSystem.instance;
      var halfZone = zoneSystem.m_zoneSize / 2.0f;
      return zones.Where(zone => {
        var center = zoneSystem.GetZonePos(zone);
        center.y = 0f;
        var distance = center - position;
        distance.x = Math.Abs(distance.x);
        distance.z = Math.Abs(distance.z);

        if (distance.x > halfZone + radius) return false;
        if (distance.y > halfZone + radius) return false;
        if (distance.z <= halfZone) return true;
        if (distance.z <= halfZone) return true;

        var cornerDistance_sq = (distance.x - halfZone) * (distance.x - halfZone) + (distance.z - halfZone) * (distance.z - halfZone);
        return cornerDistance_sq <= radius * radius;
      }).ToArray();
    }
  }
}