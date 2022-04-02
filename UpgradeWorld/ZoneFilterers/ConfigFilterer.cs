using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Filters zones based on the config (filter points + player safe range).</summary>
public class ConfigFilterer : ZoneFilterer {
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
    var amount = zones.Length;
    var filterPoints = Settings.GetFilterPoints();
    foreach (var filterPoint in filterPoints) {
      zones = FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
    }
    if (Player.m_localPlayer)
      zones = FilterByRange(zones, Player.m_localPlayer.transform.position, Settings.PlayerSafeDistance, 0);
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by being excluded the config");
    return zones;
  }

  private static Vector2i[] FilterByRange(Vector2i[] zones, Vector3 position, float min, float max) {
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
