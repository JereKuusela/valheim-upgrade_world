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
    var instances = zs.m_locationInstances.Where(kvp => true);
    if (locationObjects.Count > 0)
      instances = instances.Where(kvp => locationObjects.Contains(kvp.Value.m_location.m_prefabName.GetStableHashCode()));
    var includedZones = instances.Select(kvp => kvp.Key).ToHashSet();
    var amount = zones.Length;
    zones = zones.Where(includedZones.Contains).ToArray();
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by not having the location");
    return zones;
  }
}
