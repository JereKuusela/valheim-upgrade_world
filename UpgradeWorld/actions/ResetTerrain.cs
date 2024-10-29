
using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;


// Location generation only spawns them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem))]
public class ResetTerrain
{
  public static ILookup<Vector2i, ZDO>? TCZdos = null;
  public static DateTime LastUpdate = DateTime.MinValue;
  public static bool Active = false;

  // Needed so that the vegetation spawns on the reseted terrain.
  [HarmonyPatch(nameof(ZoneSystem.GetGroundData)), HarmonyPostfix]
  static void OverrideGroundDataWithDefaultHeight(ref Vector3 p)
  {
    if (!Active) return;
    p.y = WorldGenerator.instance.GetHeight(p.x, p.z);
  }
  // Needed so that the vegetation spawns on the reseted terrain.
  [HarmonyPatch(nameof(ZoneSystem.GetGroundHeight), typeof(Vector3)), HarmonyPrefix]
  static bool OverrideGroundHeightWithDefaultHeight(Vector3 p, ref float __result)
  {
    if (!Active) return true;
    __result = WorldGenerator.instance.GetHeight(p.x, p.z);
    return false;
  }

  private static Vector3 VertexToWorld(Vector3 pos, int j, int i)
  {
    pos.x += i - 32.5f;
    pos.z += j - 32.5f;
    return pos;
  }
  public static void Execute(Vector3 pos, float radius)
  {
    if (radius == 0f) return;
    if (TCZdos == null || DateTime.Now - LastUpdate > TimeSpan.FromSeconds(10))
    {
      LastUpdate = DateTime.Now;
      TCZdos = EntityOperation.GetZDOs(Settings.TerrainCompilerHash).ToLookup(zdo => ZoneSystem.GetZone(zdo.GetPosition()));
    }
    var removed = false;
    var centerZone = ZoneSystem.GetZone(pos);
    var d = (int)Math.Ceiling(radius / 64f);
    for (var i = centerZone.x - d; i <= centerZone.x + d; i++)
    {
      for (var j = centerZone.y - d; j <= centerZone.y + d; j++)
      {
        var zone = new Vector2i(i, j);
        if (TCZdos == null || !TCZdos.Contains(zone)) continue;
        var zdos = TCZdos[zone];
        ResetTerrainInZdo(pos, radius, zone, zdos.First());
        if (zdos.Count() > 1)
        {
          // Log warning of overlapping terrain controls.
          UpgradeWorld.Log.LogWarning($"Overlapping terrain controls in zone {zone}. Removing...");
          zdos = zdos.Skip(1);
          removed = true;
          foreach (var zdo in zdos)
            Helper.RemoveZDO(zdo);
        }
      }
    }
    if (removed) TCZdos = null;
  }
  private static void ResetTerrainInZdo(Vector3 pos, float radius, Vector2i zone, ZDO zdo)
  {
    var byteArray = zdo.GetByteArray(ZDOVars.s_TCData);
    if (byteArray == null) return;
    var center = ZoneSystem.GetZonePos(zone);
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
      if (j >= 0 && j <= width - 1 && i >= 0 && i <= width - 1)
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
    zdo.Set(ZDOVars.s_TCData, bytes);
  }
}