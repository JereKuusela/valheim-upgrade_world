using System.Collections.Generic;
using UnityEngine;
namespace UpgradeWorld;
public abstract class LocationOperation : ZoneOperation
{
  protected int Operated = 0;
  protected string Verb = "";
  public LocationOperation(Terminal context, FiltererParameters args) : base(context, args)
  {
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    var zoneSystem = ZoneSystem.instance;
    var locations = zoneSystem.m_locationInstances;
    if (!locations.TryGetValue(zone, out var location)) return true;
    if (zoneSystem.IsZoneLoaded(zone))
    {
      if (ExecuteLocation(zone, location)) Operated++;
      return true;
    }
    zoneSystem.PokeLocalZone(zone);
    return false;
  }
  protected abstract bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location);
  protected override void OnEnd()
  {
    var text = $"{Operation} completed. {Operated} locations {Verb}.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
  private readonly List<GameObject> spawnedObjects = [];
  /// <summary>Spawns a location to the game world.</summary>
  protected void SpawnLocation(Vector2i zone, ZoneSystem.LocationInstance location, float clearRadius)
  {
    var zoneSystem = ZoneSystem.instance;
    var root = zoneSystem.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    Helper.ClearAreaForLocation(zone, location, clearRadius);
    var resetRadius = Args.TerrainReset == 0f ? location.m_location.m_exteriorRadius : Args.TerrainReset;
    ResetTerrain.Execute(location.m_position, resetRadius);
    List<ZoneSystem.ClearArea> clearAreas = [];
    zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Ghost, spawnedObjects);
    foreach (var obj in spawnedObjects)
      Object.Destroy(obj);
    spawnedObjects.Clear();
  }
}
