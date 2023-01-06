using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they have given locations.</summary>
public class LocationFilterer : ZoneFilterer
{
  private IEnumerable<string> Ids;
  public LocationFilterer(IEnumerable<string> ids)
  {
    Ids = ids;
  }
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    var locationObjects = Ids.Select(id => id.GetStableHashCode()).ToHashSet();
    var zs = ZoneSystem.instance;
    var amount = zones.Length;
    zones = zones.Where(zone =>
    {
      if (!zs.m_locationInstances.TryGetValue(zone, out var instance)) return false;
      return locationObjects.Contains(instance.m_location.m_hash);
    }).ToArray();
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by not having the location");
    return zones;
  }
}
