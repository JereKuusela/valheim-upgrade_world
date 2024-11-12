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
  public static List<ZoneSystem.ClearArea> GetClearAreas(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    zs.m_tempClearAreas.Clear();
    if (zs.m_locationInstances.TryGetValue(zone, out var location))
    {
      if (location.m_location.m_clearArea)
        zs.m_tempClearAreas.Add(new ZoneSystem.ClearArea(location.m_position, location.m_location.m_exteriorRadius));
    }
    return zs.m_tempClearAreas;
  }
  protected void SpawnVegetation(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    ResetTerrain.Active = Args.TerrainReset != 0f;
    var clearAreas = GetClearAreas(zone);
    zs.m_tempSpawnedObjects.Clear();
    zs.PlaceVegetation(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Ghost, zs.m_tempSpawnedObjects);
    Counter += zs.m_tempSpawnedObjects.Count;
    foreach (var obj in zs.m_tempSpawnedObjects)
    {
      ResetTerrain.Execute(obj.transform.position, Args.TerrainReset);
      AddPin(obj.transform.position);
      Object.Destroy(obj);
    }
    zs.m_tempSpawnedObjects.Clear();
    ResetTerrain.Active = false;
    Object.Destroy(root);
    zs.m_zones.Remove(zone);
  }
}
