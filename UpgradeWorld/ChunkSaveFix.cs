using System.Collections.Generic;
using HarmonyLib;
namespace UpgradeWorld;

/// <summary>
/// Vanilla skips chunks with no objects, so a fully emptied area keeps its saved entry and file, and the objects return on load.
/// Such entries are dropped from the save index (as if never visited) and their files deleted after a successful save.
/// </summary>
public static class ChunkSaveFix
{
  public static readonly HashSet<ZoneSystem.ChunkIndex> Dropped = [];

  public static bool IsEmpty(ZDOMan zman, ZoneSystem.ChunkIndex chunk)
  {
    var (x, y) = ZoneSystem.GetZoneFromChunk(chunk);
    var span = ZoneSystem.c_ZonesPerChunk << chunk.m_chunkSize;
    for (var j = y; j < y + span; j++)
    {
      for (var i = x; i < x + span; i++)
      {
        var list = zman.m_objectsBySector[ZoneSystem.SectorToIndex(i, j).Sector];
        if (list != null && list.Count > 0) return false;
      }
    }
    return true;
  }
}

[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.AddObjectsPerChunk))]
public class FindEmptiedChunks
{
  static void Prefix(ZDOMan __instance, int size, byte chunkSize, int[] numZDOPerChunk)
  {
    // Called once per size, starting from the base chunks.
    if (chunkSize == 0) ChunkSaveFix.Dropped.Clear();
    var chunks = __instance.m_chunkSaveMapping.Chunks;
    var dirty = __instance.m_dirtyChunks[0];
    var step = 64 / size;
    for (var i = 0; i < numZDOPerChunk.Length; i++)
    {
      if (numZDOPerChunk[i] > 0) continue;
      var index = ZoneSystem.ChunkIndexFromXY((uint)(i % size * step), (uint)(i / size * step), chunkSize);
      if (index.Equals(ZoneSystem.ChunkPortal) || !dirty.Contains(index)) continue;
      // Entries already split into smaller chunks are dropped by vanilla.
      if (!chunks.TryGetValue(index, out var info) || info.m_sizeChanged) continue;
      if (ChunkSaveFix.IsEmpty(__instance, index))
        ChunkSaveFix.Dropped.Add(index);
    }
  }
}

[HarmonyPatch(typeof(ChunkSaveMapping), nameof(ChunkSaveMapping.Clone))]
public class DropEmptiedChunks
{
  static void Postfix(ChunkSaveMapping __result)
  {
    foreach (var index in ChunkSaveFix.Dropped)
      __result.m_chunks.Remove(index);
  }
}

[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.DeleteOldChunks))]
public class DeleteEmptiedChunkFiles
{
  // Runs only after a successful save, while the old mapping still knows the file names.
  static void Prefix(ZDOMan __instance, string path, FileHelpers.FileSource fileSource)
  {
    foreach (var index in ChunkSaveFix.Dropped)
    {
      var file = __instance.m_chunkSaveMapping.GetChunkFilename(index);
      if (!string.IsNullOrEmpty(file))
        FileHelpers.Delete(path + file, fileSource);
    }
    ChunkSaveFix.Dropped.Clear();
  }
}
