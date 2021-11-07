using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {
  public class PlaceLocations : ZoneOperation {

    public PlaceLocations(Terminal context) : base(context, Zones.GetAllZones(), new ZoneFilterer[] { new ConfigFilterer(), new LocationFilterer() }) {
      Operation = "Place location";
    }
    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        var locations = zoneSystem.m_locationInstances;
        if (locations.TryGetValue(zone, out var location))
          PlaceLocation(zone, location);
        else {
          Print("ERROR: Missing location.");
          Failed++;
        }
        return true;
      }
      zoneSystem.PokeLocalZone(zone);
      return false;
    }
    protected override void OnEnd() {
      var upgraded = ZonesToUpgrade.Length - Failed;
      Print("Upgrade completed. " + upgraded + " zones upgraded. " + Failed + " errors.");
    }
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    /// <summary>Places a location to the game world.</summary>
    private void PlaceLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
      var zoneSystem = ZoneSystem.instance;
      if (Settings.LocationsExcludePlayerBases && IsInsidePlayerBase(location)) {
        Failed++;
        if (Settings.Verbose)
          Print("Location placement failed at " + zone.ToString() + " because of a player base.");
        else
          Print("Location placement failed because of a player base (enable verbose mode to see the zone).");
        return;
      }
      var root = zoneSystem.m_zones[zone].m_root;
      var zonePos = ZoneSystem.instance.GetZonePos(zone);
      var heightmap = Zones.GetHeightmap(root);
      ClearAreaForLocation(zone, location);
      var clearAreas = new List<ZoneSystem.ClearArea>();
      spawnedObjects.Clear();
      zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Full, spawnedObjects);
      if (Settings.Verbose)
        Print("Location " + location.m_location.m_prefabName + " placed at " + zone.ToString() + ".");
    }
    /// <summary>Clears the area around the location to prevent overlapping entities.</summary>
    private void ClearAreaForLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
      if (location.m_location.m_location.m_clearArea)
        ClearZDOsWithinRadius(zone, location.m_position, location.m_location.m_exteriorRadius);
    }

    /// <summary>Returns is the location inside player base.</summary>
    private bool IsInsidePlayerBase(ZoneSystem.LocationInstance location) => EffectArea.IsPointInsideArea(location.m_position, EffectArea.Type.PlayerBase, location.m_location.m_exteriorRadius);

    /// <summary>Clears entities too close to a given position.</summary>
    private void ClearZDOsWithinRadius(Vector2i zone, Vector3 position, float radius) {
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