using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  /// <summary>First step of the upgrader: Get an array of zones determined by the upgrade command.</summary>
  public static class Filter {
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
  }
}