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
    if (Attempts == 0 && !Args.Roll()) return true;
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
  private readonly List<GameObject> spawnedObjects = new();
  /// <summary>Places a location to the game world.</summary>
  protected void PlaceLocation(Vector2i zone, ZoneSystem.LocationInstance location, bool clear, bool forceClear)
  {
    var zoneSystem = ZoneSystem.instance;
    var root = zoneSystem.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    if (clear || forceClear)
      Helper.ClearAreaForLocation(zone, location, forceClear);
    List<ZoneSystem.ClearArea> clearAreas = new();
    spawnedObjects.Clear();
    zoneSystem.PlaceLocations(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Full, spawnedObjects);
  }
}
