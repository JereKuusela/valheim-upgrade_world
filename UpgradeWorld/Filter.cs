using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  // First step of the upgrader: Get an array of zones determined by the upgrade command.
  public static class Filter {
    // Filters given zones by biomes.
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
    /**public static Vector2i[] FilterByBiomes(Vector2i[] zones, Heightmap.Biome[] included, Heightmap.Biome[] excluded) {
      var zoneSystem = ZoneSystem.instance;
      var halfZone = zoneSystem.m_zoneSize / 2.0f;
      return zones.Where(zone => {
        var shouldBeUnloaded = Patch.ZoneSystem_PokeLocalZone(zoneSystem, zone);
        var mZones = Patch.GetZones(zoneSystem);
        foreach (var key in mZones.Keys) {
          if (key.ToString() != zone.ToString()) {
            continue;
          }

          var obj = mZones[zone];
          if (obj == null) {
            Debug.Log("NO OBJ");
            break;
          }
          var root = Patch.Get<GameObject>(obj, "m_root");
          if (root == null) {
            Debug.Log("NO ROOT");
            break;
          }
          var heightmap = Zones.GetHeightmap(root);
          if (heightmap == null) {
            Debug.Log("NO HEIGHT");
            break;
          }
          var isIncluded = included.Count() == 0 || included.Any(heightmap.HaveBiome);
          var isExcluded = excluded.Any(heightmap.HaveBiome);
          if (shouldBeUnloaded) {
            Object.Destroy(root);
            mZones.Remove(zone);
          }
          return isIncluded && !isExcluded;
        }
        Debug.Log("KEY " + zone + " NOT FOUND");
        return false;
      }).ToArray();
    }
    */
    // Returns only zones that are partially within a given distance from a given position.
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
  }
}