using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Filters zones based on whether they are loaded or not.</summary>
  public class LoadedFilterer : ZoneFilterer {
    bool Enabled;
    public LoadedFilterer(bool enabled) {
      Enabled = enabled;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      if (!Enabled) return zones;
      var amount = zones.Length;
      zones = zones.Where(IsUnloaded).ToArray();
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by being loaded");
      return zones;
    }
    private bool IsUnloaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return !zoneSystem.IsZoneLoaded(zone);
    }
  }
}