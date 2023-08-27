using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they are close enough to a given zone.</summary>
public class ZoneDistanceFilterer : IZoneFilterer {
  private Vector2i Center;
  private readonly int MinDistance;
  private readonly int MaxDistance;
  public ZoneDistanceFilterer(Vector2i center, int minDistance, int maxDistance) {
    Center = center;
    MinDistance = minDistance;
    MaxDistance = maxDistance;
    if (MaxDistance == 0 && MinDistance > 0) MaxDistance = int.MaxValue;
  }
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
    var amount = zones.Length;
    zones = FilterByAdjacent(zones, Center, MinDistance, MaxDistance);
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by being at wrong distance");
    return zones;
  }
  /// <summary>Returns only zones that are within a given adjacencty to a given center zone.</summary>
  private static Vector2i[] FilterByAdjacent(Vector2i[] zones, Vector2i centerZone, int minDistance, int maxDistance) {
    return zones.Where(zone => {
      var withinMinX = Math.Abs(centerZone.x - zone.x) >= minDistance;
      var withinMinY = Math.Abs(centerZone.y - zone.y) >= minDistance;
      var withinMaxX = Math.Abs(centerZone.x - zone.x) <= maxDistance;
      var withinMaxY = Math.Abs(centerZone.y - zone.y) <= maxDistance;
      return (withinMinX || withinMinY) && withinMaxX && withinMaxY;
    }).ToArray();
  }
}
