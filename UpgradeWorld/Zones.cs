using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  // First step of the upgrader: Get an array of zones determined by the upgrade command.
  public static class Zones {

    public static GameObject GetRoot(object obj) {
      return Patch.Get<GameObject>(obj, "m_root");
    }
    public static Heightmap GetHeightmap(GameObject root) {
      return root.GetComponentInChildren<Heightmap>();
    }

    private static Vector2i[] Sort(Vector2i[] zones) {
      return zones.OrderBy(zone => zone.Magnitude()).ToArray();
    }
    // Returns an array of zones from the center zone towards edge zones within a given adjacency.
    public static Vector2i[] GetZonesToUpgrade(Vector3 position, int adjacent) {
      position.y = 0f;
      var side = 1 + (adjacent * 2);
      var zones = new Vector2i[side * side];
      var zoneSystem = ZoneSystem.instance;
      var currentZone = zoneSystem.GetZone(position);
      var index = 0;
      for (var x = -adjacent; x <= adjacent; x++) {
        for (var y = -adjacent; y <= adjacent; y++) {
          zones[index++] = currentZone + new Vector2i(x, y);
        }
      }
      return Sort(zones);
    }

    // Returns an array of zones from the center zone towards edge zones within a given distance.
    public static Vector2i[] GetZonesToUpgradeByRadius(Vector3 position, float radius) {
      position.y = 0f;
      var zoneSystem = ZoneSystem.instance;
      var adjacent = (int)Math.Ceiling(radius / zoneSystem.m_zoneSize);
      var zones = GetZonesToUpgrade(position, adjacent);
      return Filter.FilterByRange(zones, position, 0, radius);
    }
    // Returns an array of all generated zones.
    public static Vector2i[] GetAllZones() {
      var zoneSystem = ZoneSystem.instance;
      return Patch.GetGeneratedZones(zoneSystem).ToArray();
    }
    private static int WORLD_LIMIT = 165;
    // Returns an array of all generated zones.
    public static Vector2i[] GetWorldZones() {
      var zoneSystem = ZoneSystem.instance;
      var zones = new List<Vector2i>();
      for (var i = -WORLD_LIMIT; i <= WORLD_LIMIT; i++) {
        for (var j = -WORLD_LIMIT; j <= WORLD_LIMIT; j++) {
          if (i * i + j * j > WORLD_LIMIT * WORLD_LIMIT) continue;
          zones.Add(new Vector2i(i, j));
        }
      }
      return zones.ToArray();
    }
  }
}