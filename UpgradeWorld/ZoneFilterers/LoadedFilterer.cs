using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  public class LoadedFilterer : ZoneFilterer {
    bool Enabled;
    public LoadedFilterer(bool enabled) {
      Enabled = enabled;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      if (!Enabled) return zones;
      var amount = zones.Length;
      zones = zones.Where(IsUnloaded).ToArray();
      var filtered = amount - zones.Length;
      if (filtered > 0) messages.Add(filtered + " skipped by being loaded");
      return zones;
    }
    private bool IsUnloaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return !zoneSystem.IsZoneLoaded(zone);
    }
  }
}