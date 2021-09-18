using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {

  public partial class Operation {
    /// <summary>Destroys everything in zone if it's not loaded.</summary>
    public static bool Upgrade(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        return true;
      }
      _ = Patch.ZoneSystem_PokeLocalZone(zoneSystem, zone);
      if (!zoneSystem.IsZoneLoaded(zone)) {
        return false;
      }
      var mZones = Patch.GetZones(zoneSystem);
      var root = Zones.GetRoot(mZones[zone]);
      spawnedObjects.Clear();
      Object.Destroy(root);
      mZones.Remove(zone);
      return true;
    }
    private static readonly List<object> clearAreas = new List<object>();
    private static readonly List<GameObject> spawnedObjects = new List<GameObject>();
    // Base game code only places non-placed locations so this can be ran safely.
    public static void PlaceLocation(Vector2i zoneID, GameObject root, ZoneSystem.SpawnMode mode) {
      var zoneSystem = ZoneSystem.instance;
      var zonePos = ZoneSystem.instance.GetZonePos(zoneID);
      var heightmap = root.GetComponentInChildren<Heightmap>();
      clearAreas.Clear();
      spawnedObjects.Clear();
      Patch.ZoneSystem_PlaceLocations(zoneSystem, zoneID, zonePos, root.transform, heightmap, clearAreas, mode, spawnedObjects);
    }
  }
}