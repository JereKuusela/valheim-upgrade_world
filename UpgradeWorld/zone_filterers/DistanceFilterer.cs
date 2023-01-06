using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they are close enough to a given position.</summary>
public class DistanceFilterer : ZoneFilterer
{
  private Vector3 Center;
  private float MinDistance;
  private float MaxDistance;
  public DistanceFilterer(Vector3 center, float minDistance, float maxDistance)
  {
    Center = center;
    MinDistance = minDistance;
    MaxDistance = maxDistance;
  }
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    var amount = zones.Length;
    zones = FilterByDistance(zones, Center, MinDistance, MaxDistance);
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by the command");
    return zones;
  }

  /// <summary>Returns only zones which center point is included within a given range..</summary>
  private static Vector2i[] FilterByDistance(Vector2i[] zones, Vector3 position, float minDistance, float maxDistance)
  {
    var zoneSystem = ZoneSystem.instance;
    return zones.Where(zone =>
    {
      var center = zoneSystem.GetZonePos(zone);
      center.y = 0f;
      var delta = center - position;
      var withinMin = delta.sqrMagnitude >= minDistance * minDistance;
      var withinMax = maxDistance == 0 || delta.sqrMagnitude <= maxDistance * maxDistance;
      return withinMin && withinMax;
    }).ToArray();
  }
}
