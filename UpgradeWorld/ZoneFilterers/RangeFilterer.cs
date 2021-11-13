using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they are close enough to a given position.</summary>
  public class RangeFilterer : ZoneFilterer {
    private Vector3 Center;
    private float Range;
    public RangeFilterer(Vector3 center, float range) {
      Center = center;
      Range = range;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = FilterByDIstance(zones, Center, Range);
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by the command");
      return zones;
    }

    /// <summary>Returns only zones that are included within a given distance.</summary>
    // From: https://stackoverflow.com/a/402010
    private static Vector2i[] FilterByDIstance(Vector2i[] zones, Vector3 position, float distance) {
      var zoneSystem = ZoneSystem.instance;
      var halfZone = zoneSystem.m_zoneSize / 2.0f;
      return zones.Where(zone => {
        var center = zoneSystem.GetZonePos(zone);
        center.y = 0f;
        var delta = center - position;
        delta.x = Math.Abs(delta.x);
        delta.z = Math.Abs(delta.z);

        if (delta.x > halfZone + distance) return false;
        if (delta.y > halfZone + distance) return false;
        if (delta.z <= halfZone) return true;
        if (delta.z <= halfZone) return true;

        var cornerDistance_sq = (delta.x - halfZone) * (delta.x - halfZone) + (delta.z - halfZone) * (delta.z - halfZone);
        return cornerDistance_sq <= distance * distance;
      }).ToArray();
    }
  }
}