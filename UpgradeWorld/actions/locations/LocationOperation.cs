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
  /// <summary>Spawns a location to the game world.</summary>
  protected void SpawnLocation(Vector2i zone, ZoneSystem.LocationInstance location, float clearRadius)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    Helper.ClearAreaForLocation(zone, location, clearRadius);
    var resetRadius = Args.TerrainReset == 0f ? location.m_location.m_exteriorRadius : Args.TerrainReset;
    ResetTerrain.Execute(location.m_position, resetRadius);
    zs.m_tempSpawnedObjects.Clear();
    zs.m_tempClearAreas.Clear();
    zs.PlaceLocations(zone, zonePos, root.transform, heightmap, zs.m_tempClearAreas, ZoneSystem.SpawnMode.Ghost, zs.m_tempSpawnedObjects);
    foreach (var obj in zs.m_tempSpawnedObjects)
      Object.Destroy(obj);
    zs.m_tempSpawnedObjects.Clear();
    Object.Destroy(root);
    zs.m_zones.Remove(zone);
  }
}
