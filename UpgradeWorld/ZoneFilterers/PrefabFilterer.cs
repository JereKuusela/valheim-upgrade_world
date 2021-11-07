using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  public class PrefabFilterer : ZoneFilterer {
    private HashSet<Vector2i> IncludedZones;
    public PrefabFilterer(string id) {
      IncludedZones = null;
      var zdos = new List<ZDO>();
      ZDOMan.instance.GetAllZDOsWithPrefab(id, zdos);
      IncludedZones = zdos.Select(zdo => ZoneSystem.instance.GetZone(zdo.GetPosition())).Distinct().ToHashSet();
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      if (IncludedZones == null) {
        var amount = zones.Length;
        zones = zones.Where(zone => false).ToArray();
        var filtered = amount - zones.Length;
        if (filtered > 0) messages.Add(filtered + " skipped by having invalid entity id");
        return zones;
      } else {
        var amount = zones.Length;
        zones = zones.Where(IncludedZones.Contains).ToArray();
        var filtered = amount - zones.Length;
        if (filtered > 0) messages.Add(filtered + " skipped by not having the entity");
        return zones;
      }
    }
  }
}