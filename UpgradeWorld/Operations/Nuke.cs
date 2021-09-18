using System.Collections.Generic;

namespace UpgradeWorld {

  public partial class Operation {
    /// <summary>Destroys everything in zone if it's not loaded.</summary>
    public static bool NukeUnloaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        return true;
      }
      var sectorObjects = new List<ZDO>();
      Patch.ZDOMan_FindObjects(ZDOMan.instance, zone, sectorObjects);
      foreach (var zdo in sectorObjects) {
        var location = zdo.GetPosition();
        if (zoneSystem.GetZone(location) == zone) {
          ZDOMan.instance.RemoveFromSector(zdo, zone);
        }
      }
      var generated = Patch.GetGeneratedZones(zoneSystem);
      _ = generated.Remove(zone);
      return true;
    }

  }
}