using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {
  public class PlaceLocations : ZoneOperation {

    private bool ClearLocationAreas = true;
    private int Placed = 0;
    public PlaceLocations(Terminal context, bool clearLocationAreas, FiltererParameters args) : base(context, args.ForceStart) {
      Operation = "Place locations";
      ClearLocationAreas = clearLocationAreas;
      args.TargetZones = TargetZones.Generated;
      InitString = args.Print("Place locations at");
      Filterers = FiltererFactory.Create(args);
      ZonesPerUpdate = 100;
    }
    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var locations = zoneSystem.m_locationInstances;
      if (!locations.ContainsKey(zone)) return true;
      var location = locations[zone];
      if (location.m_placed) return true;
      if (zoneSystem.IsZoneLoaded(zone)) {
        PlaceLocation(zone, location);
        Placed++;
        return true;
      }
      zoneSystem.PokeLocalZone(zone);
      return false;
    }
    protected override void OnEnd() {
      var text = Operation + " completed.";
      if (Settings.Verbose) text += " " + Placed + " locations placed.";
      if (Failed > 0) text += " " + Failed + " errors.";
      Print(text);
    }
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    /// <summary>Places a location to the game world.</summary>
    private void PlaceLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
      var zoneSystem = ZoneSystem.instance;
      var root = zoneSystem.m_zones[zone].m_root;
      var zonePos = ZoneSystem.instance.GetZonePos(zone);
      var heightmap = Zones.GetHeightmap(root);
      ClearAreaForLocation(zone, location);
      var clearAreas = new List<ZoneSystem.ClearArea>();
      spawnedObjects.Clear();
      zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Full, spawnedObjects);
      if (Settings.Verbose)
        Print("Location " + location.m_location.m_prefabName + " placed at " + zone.ToString());
    }
    /// <summary>Clears the area around the location to prevent overlapping entities.</summary>
    private void ClearAreaForLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
      if (ClearLocationAreas && location.m_location.m_location.m_clearArea)
        ClearZDOsWithinDistance(zone, location.m_position, location.m_location.m_exteriorRadius);
    }

    /// <summary>Clears entities too close to a given position.</summary>
    private void ClearZDOsWithinDistance(Vector2i zone, Vector3 position, float distance) {
      var sectorObjects = new List<ZDO>();
      ZDOMan.instance.FindObjects(zone, sectorObjects);
      foreach (var zdo in sectorObjects) {
        if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid)
          continue;

        var zdoPosition = zdo.GetPosition();
        var delta = position - zdoPosition;
        delta.y = 0;
        if (delta.magnitude < distance) ZDOMan.instance.DestroyZDO(zdo);
      }
    }
  }
}