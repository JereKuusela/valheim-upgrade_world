using System.Collections.Generic;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Runs the vegetation placement code for the filtered zones.</summary>
public class AddVegetation : VegetationOperation
{
  public int Counter = 0;
  public AddVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, ids, args)
  {
    Operation = "Add vegetation";
    InitString = args.Print($"Add vegetation{Helper.LocationIdString(ids)} to");
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    if (zs.IsZoneLoaded(zone))
    {
      SpawnVegetation(zone);
      return true;
    }
    zs.PokeLocalZone(zone);
    return false;
  }
  protected override void OnStart()
  {
    base.OnStart();
  }
  protected override void OnEnd()
  {
    base.OnEnd();
    var text = $"{Operation} completed. {Counter} vegetations added.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
  private List<ZoneSystem.ClearArea> GetClearAreas(Vector2i zone)
  {
    List<ZoneSystem.ClearArea> clearAreas = [];
    var zs = ZoneSystem.instance;
    if (zs.m_locationInstances.TryGetValue(zone, out var location))
    {
      if (location.m_location.m_location.m_clearArea)
        clearAreas.Add(new ZoneSystem.ClearArea(location.m_position, location.m_location.m_exteriorRadius));
    }
    return clearAreas;
  }
  private readonly List<GameObject> spawnedObjects = [];
  protected void SpawnVegetation(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    ResetTerrain.ResetRadius = Args.TerrainReset;
    var clearAreas = GetClearAreas(zone);
    zs.PlaceVegetation(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Ghost, spawnedObjects);
    Counter += spawnedObjects.Count;
    foreach (var obj in spawnedObjects)
    {
      AddPin(obj.transform.position);
      Object.Destroy(obj);
    }
    spawnedObjects.Clear();
    ResetTerrain.ResetRadius = 0f;
  }
}
