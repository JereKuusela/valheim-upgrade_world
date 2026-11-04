using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld;

public class RefillSnow : ZoneOperation
{
  private readonly ILookup<Vector2s, ZDOID> terrain;
  private readonly ZNet network = ZNet.instance;
  private readonly long session = ZDOMan.GetSessionID();
  private readonly SnowResetAreas areas = new();
  private readonly HashSet<ZDOID> remaining = [];
  private readonly HashSet<ZDOID> inspected = [];
  private int changedRecords, changedPoints;

  public RefillSnow(Terminal context, FiltererParameters args) : base(context, args)
  {
    Operation = "Refill snow";
    InitString = args.Print("Refill Deep North snow in");
    Filterers = FiltererFactory.Create(args);
    // Saved terrain can exist before the zone is marked generated.
    terrain = EntityOperation.GetZDOs(Settings.TerrainCompilerHash)
      .ToLookup(zdo => ZoneSystem.GetZone(zdo.GetPosition()), zdo => zdo.m_uid);
    ZonesToUpgrade = terrain.Select(group => group.Key)
      .Where(zone => !args.Zone.HasValue || args.MinDistance != 0f || args.MaxDistance != 0f || zone == args.Zone.Value)
      .OrderBy(zone => zone.Magnitude()).ToArray();
    // Check each paint sample instead of excluding whole biome-edge zones.
    Filterers.RemoveAll(filter => filter is BiomeFilterer);
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    CheckSession();
    foreach (var zone in ZonesToUpgrade)
      foreach (var id in terrain[zone]) remaining.Add(id);
    var total = remaining.Count;
    var lastWaiting = -1;
    while (remaining.Count > 0)
    {
      foreach (var zone in ZonesToUpgrade)
      {
        CheckSession();
        ExecuteZone(zone);
        if (sw.ElapsedMilliseconds >= Executor.ProgressMin)
        {
          PrintOnce($"Refill snow: {total - remaining.Count}/{total} terrain records", false);
          yield return null;
          CheckSession();
          sw.Restart();
        }
      }
      if (remaining.Count == 0) yield break;
      if (lastWaiting != remaining.Count)
        Print($"Snow reset is waiting for {remaining.Count} terrain records used or cached by connected players. Use stop to cancel");
      lastWaiting = remaining.Count;
      ZonesToUpgrade = ZonesToUpgrade.Where(zone => terrain[zone].Any(remaining.Contains)).ToArray();
      yield return new WaitForSeconds(1f);
      CheckSession();
      sw.Restart();
    }
  }

  private void CheckSession()
  {
    if (!network || network != ZNet.instance || session != ZDOMan.GetSessionID())
    {
      User = null;
      throw new InvalidOperationException("The snow reset's world session ended");
    }
    if (User != null && !network.GetPeers().Any(peer => peer.m_rpc == User)) User = null;
  }

  protected override bool ExecuteZone(Vector2s zone)
  {
    HeightmapBuilder.HMBuildData? natural = null;
    foreach (var id in terrain[zone])
    {
      if (!remaining.Contains(id)) continue;
      var zdo = ZDOMan.instance.GetZDO(id);
      if (zdo == null) { remaining.Remove(id); continue; }
      var canModify = areas.CanModify(zdo);
      if (!canModify && inspected.Contains(id)) continue;
      var compressed = zdo.GetByteArray(ZDOVars.s_TCData);
      if (compressed == null) { remaining.Remove(id); continue; }
      inspected.Add(id);
      var center = zdo.GetPosition();
      var raw = Utils.Decompress(compressed);
      var result = SnowResetData.Refill(raw, NaturalSnow, out var points);
      if (result == null) { remaining.Remove(id); continue; }
      // Unchanged records can finish without waiting; pending edits are reread later.
      if (!canModify) continue;
      var payload = Utils.Compress(result);
      var owner = zdo.GetOwner();
      try
      {
        // A disconnected owner's record can still trigger server-write protection.
        if (owner != 0L) zdo.SetOwner(ZDOMan.GetSessionID());
        zdo.Set(ZDOVars.s_TCData, payload);
      }
      finally
      {
        zdo.SetOwner(owner);
      }
      SnowResetSync.Refresh(zdo);
      changedRecords++;
      changedPoints += points;
      remaining.Remove(id);

      float? NaturalSnow(int x, int y, int width)
      {
        var scale = 64f / width;
        var position = center + new Vector3((x - width / 2f - 0.5f) * scale, 0f, (y - width / 2f - 0.5f) * scale);
        if (WorldGenerator.instance.GetBiome(position) != Heightmap.Biome.DeepNorth) return null;
        if (Args.NoEdges && WorldGenerator.instance.GetBiomeArea(position) != Heightmap.BiomeArea.Median) return null;
        if (natural == null || natural.m_center != center || natural.m_width != width)
          natural = HeightmapBuilder.instance.RequestTerrainSync(center, width, scale, false, WorldGenerator.instance);
        return natural.m_baseMask[y * (width + 1) + x].g;
      }
    }
    return true;
  }

  protected override void OnEnd()
  {
    Print($"Refilled {changedPoints} snow points in {changedRecords} terrain records ({inspected.Count} inspected). Saved height edits and dirt/paved paint were preserved");
    if (inspected.Count == 0) Print("No saved terrain records matched these filters");
    else if (changedPoints == 0) Print("No missing snow was found outside the protected terrain points");
  }
}
