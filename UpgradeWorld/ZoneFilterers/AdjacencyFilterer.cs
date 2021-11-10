using System;
using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they are close enough to a given zone.</summary>
  public class AdjacencyFilterer : ZoneFilterer {
    private Vector2i Center;
    private int Adjacent;
    public AdjacencyFilterer(Vector2i center, int adjacent) {
      Center = center;
      Adjacent = adjacent;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = FilterByAdjacent(zones, Center, Adjacent);
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by the command");
      return zones;
    }
    /// <summary>Returns only zones that are within a given adjacencty to a given center zone.</summary>
    private static Vector2i[] FilterByAdjacent(Vector2i[] zones, Vector2i centerZone, int adjacent) {
      return zones.Where(zone => {
        var withinX = Math.Abs(centerZone.x - zone.x) <= adjacent;
        var withinY = Math.Abs(centerZone.y - zone.y) <= adjacent;
        return withinX && withinY;
      }).ToArray();
    }
  }
}