using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they have given locations.</summary>
public class LocationFilterer(IEnumerable<string> Ids, bool OnlyMissing) : IZoneFilterer
{
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    var locationObjects = Ids.Select(id => id.GetStableHashCode()).ToHashSet();
    var zs = ZoneSystem.instance;
    var amount = zones.Length;
    zones = [.. zones.Where(zone =>
      zs.m_locationInstances.TryGetValue(zone, out var instance) &&
      (!OnlyMissing || !instance.m_placed) &&
      locationObjects.Contains(instance.m_location.Hash)
    )];
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by not having the location");
    return zones;
  }
}
