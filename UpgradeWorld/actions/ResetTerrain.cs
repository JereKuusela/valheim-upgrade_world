
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;


// Location generation only spawns them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem))]
public class ResetTerrain
{
  public static Dictionary<Vector2i, ZDO> TCZdos = new();
  public static DateTime LastUpdate = DateTime.MinValue;
  public static float ResetRadius = 0f;

  // Needed so that the vegetation spawns on the reseted terrain.
  [HarmonyPatch(nameof(ZoneSystem.GetGroundData)), HarmonyPostfix]
  static void OverrideGroundDataWithDefaultHeight(ref Vector3 p)
  {
    if (ResetRadius == 0f) return;
    p.y = WorldGenerator.instance.GetHeight(p.x, p.z);
  }
  // Needed so that the vegetation spawns on the reseted terrain.
  [HarmonyPatch(nameof(ZoneSystem.GetGroundHeight), typeof(Vector3)), HarmonyPrefix]
  static bool OverrideGroundHeightWithDefaultHeight(Vector3 p, ref float __result)
  {
    if (ResetRadius == 0f) return true;
    __result = WorldGenerator.instance.GetHeight(p.x, p.z);
    return false;
  }
  // Reset done here because this is the last check.
  [HarmonyPatch(nameof(ZoneSystem.InsideClearArea)), HarmonyPostfix]
  static void ResetTerrainBeforeVegetation(bool __result, Vector3 point)
  {
    if (__result) return;
    if (ResetRadius == 0f) return;
    Execute(point, ResetRadius);
  }

  private static Vector3 VertexToWorld(Vector3 pos, int j, int i)
  {
    pos.x += i - 32.5f;
    pos.z += j - 32.5f;
    return pos;
  }
  public static void Execute(Vector3 pos) => Execute(pos, ResetRadius);
  private static void Execute(Vector3 pos, float radius)
  {
    if (radius == 0f) return;
    var zone = ZoneSystem.instance.GetZone(pos);
    if (DateTime.Now - LastUpdate > TimeSpan.FromSeconds(10))
    {
      LastUpdate = DateTime.Now;
      TCZdos = EntityOperation.GetZDOs(Settings.TerrainCompilerHash).ToDictionary(zdo => ZoneSystem.instance.GetZone(zdo.GetPosition()));
    }
    if (!TCZdos.TryGetValue(zone, out var zdo)) return;
    var byteArray = zdo.GetByteArray("TCData");
    if (byteArray == null) return;
    var center = ZoneSystem.instance.GetZonePos(zone);
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
        var worldPos = VertexToWorld(center, j, i);
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
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var j = index / width;
      var i = index % width;
      var worldPos = VertexToWorld(center, j, i);
      if (Utils.DistanceXZ(worldPos, pos) < radius)
        modified = false;
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        change = true;
        from.ReadSingle();
        from.ReadSingle();
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    var bytes = Utils.Compress(to.GetArray());
    if (!change) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.GetSessionID());
    zdo.Set("TCData", bytes);
  }
}