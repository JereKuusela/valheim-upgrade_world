using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld;

/// This was combined from remove and add so that chance works correctly with zones.
public class ResetVegetation : VegetationOperation
{
  public int Counter = 0;
  protected int Removed = 0;
  public HashSet<int> Hashes = [];
  public ResetVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, ids, args)
  {
    ZonesPerUpdate = Settings.DestroysPerUpdate;
    Operation = "Reset vegetation";
    InitString = args.Print($"Reset vegetation{Helper.IdString(ids)} from");
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
    // No parameter -> all vegetation.
    if (ids.Count == 0)
      ids = ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).ToHashSet();
    // Automatically clean up fractions as well.
    Hashes = ids.Select(id => id.GetStableHashCode()).ToHashSet();
    // Automatically clean up fractions as well.
    foreach (var id in ids)
    {
      if (ZNetScene.instance.GetPrefab(id + "_frac"))
        Hashes.Add((id + "_frac").GetStableHashCode());
    }
  }
  protected override bool ExecuteZone(Vector2i zone)
  {

    var zs = ZoneSystem.instance;
    if (zs.IsZoneLoaded(zone))
    {
      Remove(zone);
      SpawnVegetation(zone);
      return true;
    }
    zs.PokeLocalZone(zone);
    return false;
  }
  protected void Remove(Vector2i zone)
  {
    var zdos = Helper.GetZDOs(zone);
    if (zdos == null) return;
    foreach (var zdo in zdos)
    {
      if (!Args.Roll()) continue;
      if (!Hashes.Contains(zdo.m_prefab)) continue;
      AddPin(zdo.GetPosition());
      Helper.RemoveZDO(zdo);
      Removed++;
    }
  }
  private readonly List<GameObject> spawnedObjects = [];
  protected void SpawnVegetation(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    ResetTerrain.Active = Args.TerrainReset != 0f;
    var clearAreas = AddVegetation.GetClearAreas(zone);
    zs.PlaceVegetation(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Ghost, spawnedObjects);
    Counter += spawnedObjects.Count;
    foreach (var obj in spawnedObjects)
    {
      ResetTerrain.Execute(obj.transform.position, Args.TerrainReset);
      AddPin(obj.transform.position);
      Object.Destroy(obj);
    }
    spawnedObjects.Clear();
    ResetTerrain.Active = false;
  }
  protected override void OnEnd()
  {
    var text = $"{Operation} completed. {Removed} vegetations removed. {Counter} vegetations added.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}