using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they have a given prefab.</summary>
public class PrefabFilterer(string id) : IZoneFilterer
{
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    HashSet<Vector2i> IncludedZones;
    var hash = id.GetStableHashCode();
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.m_prefab == hash);
    IncludedZones = [.. zdos.Select(zdo => ZoneSystem.GetZone(zdo.GetPosition())).Distinct()];
    if (IncludedZones == null)
    {
      var amount = zones.Length;
      zones = [.. zones.Where(zone => false)];
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by having invalid entity id");
      return zones;
    }
    else
    {
      var amount = zones.Length;
      zones = [.. zones.Where(IncludedZones.Contains)];
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by not having the entity");
      return zones;
    }
  }
}
