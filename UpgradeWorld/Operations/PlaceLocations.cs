using System.Collections.Generic;
using UnityEngine;
namespace UpgradeWorld;
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
  private readonly List<GameObject> spawnedObjects = new();
  /// <summary>Places a location to the game world.</summary>
  private void PlaceLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
    var zoneSystem = ZoneSystem.instance;
    var root = zoneSystem.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    if (ClearLocationAreas)
      Helper.ClearAreaForLocation(zone, location);
    List<ZoneSystem.ClearArea> clearAreas = new();
    spawnedObjects.Clear();
    zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Full, spawnedObjects);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " placed at " + zone.ToString());
  }
}
