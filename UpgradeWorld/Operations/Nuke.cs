using System.Collections.Generic;

namespace UpgradeWorld {

  public partial class Operation {
    /// <summary>Destroys everything in zone if it's not loaded.</summary>
    public static void NukeUnloaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        return;
      }
      var sectorObjects = new List<ZDO>();
      Patch.ZDOMan_FindObjects(ZDOMan.instance, zone, sectorObjects);
      foreach (var zdo in sectorObjects) {
        var position = zdo.GetPosition();
        if (zoneSystem.GetZone(position) == zone) {
          ZDOMan.instance.RemoveFromSector(zdo, zone);
        }
      }
      var locations = zoneSystem.m_locationInstances;
      if (locations.TryGetValue(zone, out var location)) {
        location.m_placed = false;
        zoneSystem.m_locationInstances[zone] = location;
      }
      var generated = Patch.GetGeneratedZones(zoneSystem);
      _ = generated.Remove(zone);
    }

  }
}