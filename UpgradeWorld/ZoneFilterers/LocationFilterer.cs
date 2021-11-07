using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  public class LocationFilterer : ZoneFilterer {
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      zones = zones.Where(HasMisingLocation).ToArray();
      var filtered = amount - zones.Length;
      if (filtered > 0) messages.Add(filtered + " by not having an unplaced location");
      return zones;
    }
    private bool HasMisingLocation(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var locations = zoneSystem.m_locationInstances;
      return locations.TryGetValue(zone, out var location) && !location.m_placed;
    }
  }
}