using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public class RangeFilterer : ZoneFilterer {
    private Vector3 Center;
    private float Range;
    public RangeFilterer(Vector3 center, float range) {
      Center = center;
      Range = range;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = FilterByRange(zones, Center, Range);
      var filtered = amount - zones.Length;
      if (filtered > 0) messages.Add(filtered + " by the command");
      return zones;
    }

    /// <summary>Returns only zones that are included within a given distance.</summary>
    // From: https://stackoverflow.com/a/402010
    private static Vector2i[] FilterByRange(Vector2i[] zones, Vector3 position, float radius) {
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

    Vector2i[] ZoneFilterer.FilterZones(Vector2i[] zones, ref List<string> messages) {
      throw new NotImplementedException();
    }
  }
}