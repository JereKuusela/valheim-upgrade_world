using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Runs the vegetation placement code for the filtered zones.</summary>
public class AddVegetation : VegetationOperation
{
  public static Dictionary<Vector2i, ZDO> TCZdos = new();
  public static DateTime LastUpdate = DateTime.MinValue;
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
      Place(zone);
      return true;
    }
    zs.PokeLocalZone(zone);
    return false;
  }
  protected override void OnStart()
  {
    base.OnStart();
    if (Args.TerrainReset != 0f && DateTime.Now - LastUpdate > TimeSpan.FromSeconds(10))
    {
      LastUpdate = DateTime.Now;
      TCZdos = EntityOperation.GetZDOs(Settings.TerrainCompilerId).ToDictionary(zdo => ZoneSystem.instance.GetZone(zdo.GetPosition()));
    }
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
    List<ZoneSystem.ClearArea> clearAreas = new();
    var zs = ZoneSystem.instance;
    if (zs.m_locationInstances.TryGetValue(zone, out var location))
    {
      if (location.m_location.m_location.m_clearArea)
        clearAreas.Add(new ZoneSystem.ClearArea(location.m_position, location.m_location.m_exteriorRadius));
    }
    return clearAreas;
  }
  private readonly List<GameObject> spawnedObjects = new();
  protected void Place(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.instance.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    if (Args.TerrainReset != 0f)
    {
      VegetationResetTerrain.Heightmap = heightmap;
      VegetationResetTerrain.ResetRadius = Args.TerrainReset;
      if (TCZdos.TryGetValue(zone, out var zdo))
        VegetationResetTerrain.TCZDO = zdo;
    }
    var clearAreas = GetClearAreas(zone);
    zs.PlaceVegetation(zone, zonePos, root.transform, heightmap, clearAreas, ZoneSystem.SpawnMode.Ghost, spawnedObjects);
    Counter += spawnedObjects.Count;
    foreach (var obj in spawnedObjects)
      UnityEngine.Object.Destroy(obj);
    spawnedObjects.Clear();
    VegetationResetTerrain.Heightmap = null;
    VegetationResetTerrain.ResetRadius = 0f;
    VegetationResetTerrain.TCZDO = null;
  }
}

// Location generation only places them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem))]
public class VegetationResetTerrain
{
  public static float ResetRadius = 0f;
  public static Heightmap? Heightmap = null;
  public static ZDO? TCZDO = null;

  // Needed so that the vegetation spawns on the reseted terrain.
  [HarmonyPatch(nameof(ZoneSystem.GetGroundData)), HarmonyPostfix]
  static void OverrideGroundWithDefaultHeight(ref Vector3 p)
  {
    if (ResetRadius == 0f) return;
    if (Heightmap == null) return;
    if (TCZDO == null) return;
    p.y = WorldGenerator.instance.GetHeight(p.x, p.z);
  }
  // Reset done here because this is the last check.
  [HarmonyPatch(nameof(ZoneSystem.InsideClearArea)), HarmonyPostfix]
  static void ResetTerrainBeforeVegetation(bool __result, Vector3 point)
  {
    if (__result) return;
    if (ResetRadius == 0f) return;
    if (Heightmap == null) return;
    if (TCZDO == null) return;
    ResetTerrain(Heightmap, TCZDO, point, ResetRadius);
  }


  private static Vector3 VertexToWorld(Heightmap hmap, int j, int i)
  {
    var vector = hmap.transform.position;
    vector.x += (i - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    vector.z += (j - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    return vector;
  }
  private static void ResetTerrain(Heightmap heightmap, ZDO zdo, Vector3 pos, float radius)
  {
    var byteArray = zdo.GetByteArray("TCData");
    if (byteArray == null)
      return;
    var change = false;
    var from = new ZPackage(Utils.Decompress(byteArray));
    var to = new ZPackage();
    to.Write(from.ReadInt());
    to.Write(from.ReadInt() + 1);
    to.Write(from.ReadVector3());
    to.Write(from.ReadSingle());
    var size = from.ReadInt();
    to.Write(size);
    var width = (int)Math.Sqrt(size);
    for (int index = 0; index < size; index++)
    {
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var j = index / width;
      var i = index % width;
      // Skip borders since that will cause issues with adjacent zones.
      if (j > 0 && j < width - 1 && i > 0 && i < width - 1)
      {
        var worldPos = VertexToWorld(heightmap, j, i);
        if (Utils.DistanceXZ(worldPos, pos) < radius)
          modified = false;
      }
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        change = true;
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    size = from.ReadInt();
    to.Write(size);
    for (int index = 0; index < size; index++)
    {
      var modified = from.ReadBool();
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
    }
    var bytes = Utils.Compress(to.GetArray());
    if (!change) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    zdo.Set("TCData", bytes);
  }
}