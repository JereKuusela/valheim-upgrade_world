using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they include a player base item.</summary>
  public class PlayerBaseFilterer : ZoneFilterer {
    public HashSet<Vector2i> ExcludedZones = new HashSet<Vector2i>();
    public PlayerBaseFilterer() {
      var ids = Settings.SafeZoneItems;
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => ids.Contains(zdo.GetPrefab()));
      var adjacent = Settings.SafeZoneSize;
      foreach (var zdo in zdos) {
        var zone = ZoneSystem.instance.GetZone(zdo.GetPosition());
        for (var i = -adjacent; i <= adjacent; i++) {
          for (var j = -adjacent; j <= adjacent; j++) {
            ExcludedZones.Add(new Vector2i(zone.x + i, zone.y + j));
          }
        }
      }
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = zones.Where(zone => !ExcludedZones.Contains(zone)).ToArray();
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by having a player base");
      return zones;
    }
  }
}