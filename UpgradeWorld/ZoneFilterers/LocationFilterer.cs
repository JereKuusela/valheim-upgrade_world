using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they have an unplaced location.</summary>
  public class LocationFilterer : ZoneFilterer {
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = zones.Where(HasMisingLocation).ToArray();
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " by not having an unplaced location");
      return zones;
    }
    private bool HasMisingLocation(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var locations = zoneSystem.m_locationInstances;
      return locations.TryGetValue(zone, out var location) && !location.m_placed;
    }
  }
}