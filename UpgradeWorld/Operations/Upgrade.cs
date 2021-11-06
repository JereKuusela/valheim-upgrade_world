using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {

  public partial class Operations {
    public static bool NeedsUpgrade(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var locations = zoneSystem.m_locationInstances;
      return locations.TryGetValue(zone, out var location) && !location.m_placed;
    }
    public static bool Upgrade(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        UpgradeLoaded(zone);
        return true;
      }
      zoneSystem.PokeLocalZone(zone);
      if (!zoneSystem.IsZoneLoaded(zone)) {
        return false;
      }
      UpgradeUnloaded(zone);
      return true;
    }
    private static void UpgradeLoaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var root = zoneSystem.m_zones[zone].m_root;
      PlaceLocation(zone, root, ZoneSystem.SpawnMode.Full);
    }
    private static void UpgradeUnloaded(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var root = zoneSystem.m_zones[zone].m_root;
      PlaceLocation(zone, root, ZoneSystem.SpawnMode.Ghost);
      foreach (var obj in spawnedObjects) {
        Object.Destroy(obj);
      }
      spawnedObjects.Clear();
      Object.Destroy(root);
      zoneSystem.m_zones.Remove(zone);
    }
    private static readonly List<GameObject> spawnedObjects = new List<GameObject>();
    /// <summary>Places a location to the game world.</summary>
    private static void PlaceLocation(Vector2i zone, GameObject root, ZoneSystem.SpawnMode mode) {
      var zoneSystem = ZoneSystem.instance;
      var zonePos = ZoneSystem.instance.GetZonePos(zone);
      var heightmap = Zones.GetHeightmap(root);
      ClearAreaForLocation(zone);
      var clearAreas = new List<ZoneSystem.ClearArea>();
      spawnedObjects.Clear();
      zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, mode, spawnedObjects);
    }
    /// <summary>Clears the area around the location to prevent overlapping entities.</summary>
    private static void ClearAreaForLocation(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var locations = zoneSystem.m_locationInstances;
      if (locations.TryGetValue(zone, out var location)) {
        if (location.m_location.m_location.m_clearArea) {
          var radius = location.m_location.m_exteriorRadius;
          ClearZDOsWithinRadius(zone, location.m_position, radius);
        }
      }

    }
    /// <summary>Clears entities too close to a given position.</summary>
    private static void ClearZDOsWithinRadius(Vector2i zone, Vector3 position, float radius) {
      var sectorObjects = new List<ZDO>();
      ZDOMan.instance.FindObjects(zone, sectorObjects);
      foreach (var zdo in sectorObjects) {
        if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid) {
          continue;
        }

        var zdoPosition = zdo.GetPosition();
        var delta = position - zdoPosition;
        delta.y = 0;
        if (delta.magnitude < radius) {
          ZDOMan.instance.RemoveFromSector(zdo, zone);
        }
      }
    }
  }
}