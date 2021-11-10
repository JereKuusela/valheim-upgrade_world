using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they have a given prefab.</summary>
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
        var skipped = amount - zones.Length;
        if (skipped > 0) messages.Add(skipped + " skipped by having invalid entity id");
        return zones;
      } else {
        var amount = zones.Length;
        zones = zones.Where(IncludedZones.Contains).ToArray();
        var skipped = amount - zones.Length;
        if (skipped > 0) messages.Add(skipped + " skipped by not having the entity");
        return zones;
      }
    }
  }
}