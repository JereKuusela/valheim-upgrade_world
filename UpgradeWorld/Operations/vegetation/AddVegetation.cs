using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Runs the vegetation placement code for the filtered zones.</summary>
public class AddVegetation : VegetationOperation {
  public AddVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, ids, args) {
    Operation = "Add vegetation";
    InitString = args.Print("Add vegetation at");
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone) {
    var zs = ZoneSystem.instance;
    if (zs.IsZoneLoaded(zone)) {
      Place(zone);
      return true;
    }
    zs.PokeLocalZone(zone);
    return false;
  }
  protected override void OnStart() {
    base.OnStart();
    CountNewEntities.Counter = 0;
  }

  protected override void OnEnd() {
    base.OnEnd();
    var text = Operation + " completed.";
    if (Settings.Verbose) text += $" {CountNewEntities.Counter} vegetations added.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
    CountNewEntities.Counter = 0;
  }
  private List<ZoneSystem.ClearArea> GetClearAreas(Vector2i zone) {
    List<ZoneSystem.ClearArea> clearAreas = new();
    var zs = ZoneSystem.instance;
    if (zs.m_locationInstances.TryGetValue(zone, out var location)) {
      if (location.m_location.m_location.m_clearArea)
        clearAreas.Add(new ZoneSystem.ClearArea(location.m_position, location.m_location.m_exteriorRadius));
    }
    return clearAreas;
  }
  private readonly List<GameObject> spawnedObjects = new();
  protected void Place(Vector2i zone) {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    var clearAreas = GetClearAreas(zone);
    spawnedObjects.Clear();
    zs.PlaceVegetation(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Full, spawnedObjects);
  }
}

[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.CreateNewZDO), new[] { typeof(ZDOID), typeof(Vector3) })]
public class CountNewEntities {
  public static int Counter = 0;
  static void Prefix() {
    Counter++;
  }
}
