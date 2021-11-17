using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  // First step of the upgrader: Get an array of zones determined by the upgrade command.
  public static class Zones {
    public static Heightmap GetHeightmap(GameObject root) {
      return root.GetComponentInChildren<Heightmap>();
    }

    private static Vector2i[] Sort(Vector2i[] zones) {
      return zones.OrderBy(zone => zone.Magnitude()).ToArray();
    }
    private static int WORLD_LIMIT = 165;
    // Returns an array of all ungenerated zones.
    public static Vector2i[] GetWorldZones() {
      var zoneSystem = ZoneSystem.instance;
      var zones = new List<Vector2i>();
      for (var i = -WORLD_LIMIT; i <= WORLD_LIMIT; i++) {
        for (var j = -WORLD_LIMIT; j <= WORLD_LIMIT; j++) {
          if (i * i + j * j > WORLD_LIMIT * WORLD_LIMIT) continue;
          zones.Add(new Vector2i(i, j));
        }
      }
      return Sort(zones.ToArray());
    }

    public static int Distance(Vector2i a, Vector2i b) => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
    public static bool IsWithin(Vector2i a, Vector2i b, int min, int max) {
      var distance = Distance(a, b);
      return distance >= min && (max == 0 || distance <= max);
    }
  }
}