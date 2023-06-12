using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they include a player base item.</summary>
public class PlayerBaseFilterer : IZoneFilterer {
  public static HashSet<Vector2i> ExcludedZones = new();
  public static DateTime LastUpdate = DateTime.MinValue;
  public static int LastSize = 0;
  public int Size = 0;
  public PlayerBaseFilterer(int size) {
    Size = size;
  }
  private HashSet<Vector2i> GetExcluded() {
    if (Size != LastSize || DateTime.Now - LastUpdate > TimeSpan.FromSeconds(10)) {
      ExcludedZones = CalculateExcluded(Size);
      LastUpdate = DateTime.Now;
      LastSize = Size;
    }
    return ExcludedZones;
  }
  private static HashSet<Vector2i> CalculateExcluded(int size) {
    HashSet<Vector2i> excludedZones = new();
    if (size == 0) return excludedZones;
    var adjacent = size - 1;
    var ids = Settings.SafeZoneItems;
    var allIds = Settings.SafeZoneObjects;
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => allIds.Contains(zdo.GetPrefab()) || (ids.Contains(zdo.GetPrefab()) && zdo.GetLong("creator") != 0L));
    foreach (var zdo in zdos) {
      var zone = ZoneSystem.instance.GetZone(zdo.GetPosition());
      for (var i = -adjacent; i <= adjacent; i++) {
        for (var j = -adjacent; j <= adjacent; j++) {
          excludedZones.Add(new(zone.x + i, zone.y + j));
        }
      }
    }
    return excludedZones;
  }
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
    var excludedZones = GetExcluded();
    var amount = zones.Length;
    zones = zones.Where(zone => !excludedZones.Contains(zone)).ToArray();
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by having a player base");
    return zones;
  }
}
