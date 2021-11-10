using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  public enum TargetZones {
    Generated,
    Ungenerated,
    All
  }

  ///<summary>Filters zones based on whether they are generated or not.</summary>
  public class TargetZonesFilterer : ZoneFilterer {
    TargetZones TargetZones;
    public TargetZonesFilterer(TargetZones targetZones) {
      TargetZones = targetZones;
    }
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      if (TargetZones == TargetZones.All) return zones;
      var amount = zones.Length;
      zones = zones.Where(IsValid).ToArray();
      var skipped = amount - zones.Length;
      if (skipped > 0) messages.Add(skipped + " skipped by " + (TargetZones == TargetZones.Generated ? "not" : "already") + " being generated");
      return zones;
    }
    private bool IsValid(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (TargetZones == TargetZones.Generated) return zoneSystem.IsZoneGenerated(zone);
      if (TargetZones == TargetZones.Ungenerated) return !zoneSystem.IsZoneGenerated(zone);
      return true;
    }
  }
}