using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;

public class RefillSnow(Terminal context, FiltererParameters args, bool live) : ExecutedOperation(context)
{
  public RefillSnow(Terminal context, FiltererParameters args) : this(context, args, false) { }

  private static Action<TerrainComp>? checkLoad;
  private static Func<TerrainComp, int>? paintHash;
  private static FieldInfo? lastHash;
  private readonly ZNet network = ZNet.instance;
  private readonly long session = ZDOMan.GetSessionID();
  private readonly SnowResetAreas areas = new();
  private readonly Queue<ZDOID> pending = new();
  private int total, changedRecords, changedPoints, skipped, unconfirmed;
  private int previewFrame;
  private float nextProgress;
  private bool firstPass = true;
  private string? skipMessage;
  internal int PendingCount => pending.Count;
  internal override bool IsComplete => pending.Count == 0;

  protected override string OnInit()
  {
    previewFrame = Time.frameCount;
    var zones = SelectTerrain(out var messages);
    if (pending.Count == 0)
    {
      Print("No saved terrain records matched these filters");
      return "";
    }
    var zoneLabel = zones == 1 ? "candidate zone" : "candidate zones";
    var summary = $"{args.Print(live ? "Refill snow live in" : "Refill snow with delay in")}\n{zones} {zoneLabel}";
    if (messages.Count > 0) summary += $" ({Helper.JoinRows(messages)})";
    return summary + ".";
  }

  private int SelectTerrain(out List<string> messages)
  {
    // Terrain records can exist in ungenerated zones.
    var exactZone = args.MinDistance == 0f && args.MaxDistance == 0f ? args.Zone : null;
    var records = exactZone is { } zone
      ? (Helper.GetZDOs(zone) ?? []).Where(zdo => zdo.GetPrefab() == Settings.TerrainCompilerHash && ZoneSystem.GetZone(zdo.GetPosition()) == zone)
      : EntityOperation.GetZDOs(Settings.TerrainCompilerHash);
    var terrain = records.ToLookup(zdo => ZoneSystem.GetZone(zdo.GetPosition()), zdo => zdo.m_uid);
    var zones = terrain.Select(group => group.Key).OrderBy(zone => zone.Magnitude()).ToArray();
    // Zone corner checks can miss Deep North paint at biome edges.
    messages = [];
    foreach (var filter in FiltererFactory.Create(args))
      if (filter is not BiomeFilterer) zones = filter.FilterZones(zones, ref messages);

    pending.Clear();
    foreach (var selected in zones)
      foreach (var id in terrain[selected]) pending.Enqueue(id);
    total = pending.Count;
    return zones.Length;
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    CheckSession();
    if (live && firstPass && Time.frameCount != previewFrame && args.Chance >= 1f) SelectTerrain(out _);
    var count = pending.Count;
    for (var index = 0; index < count; index++)
    {
      var id = pending.Dequeue();
      if (!TryFinishRecord(id)) pending.Enqueue(id);
      if (sw.ElapsedMilliseconds < Executor.ProgressMin) continue;
      if (Time.realtimeSinceStartup >= nextProgress)
      {
        PrintOnce($"Refill snow {total - pending.Count}/{total} terrain records", false);
        nextProgress = Time.realtimeSinceStartup + 1f;
      }
      yield return null;
      CheckSession();
      sw.Restart();
    }
    if (live)
    {
      SnowResetLive.RequestSceneReplies();
      if (firstPass && pending.Count > 0) Print($"Syncing client terrain for {pending.Count} records");
    }
    else if (pending.Count > 0 && (firstPass || pending.Count != count))
      Print($"Delayed snow refill is waiting for {pending.Count} terrain records. Players may need to reconnect away from the area");
    firstPass = false;
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

  private bool TryFinishRecord(ZDOID id)
  {
    var replaced = false;
    var handoff = live && SnowResetLive.HasHandoff(id, out replaced);
    if (handoff && replaced)
    {
      if (!SnowResetLive.SceneReady(id, out var confirmed)) return false;
      if (!confirmed) unconfirmed++;
      return true;
    }
    if (handoff)
    {
      if (!SnowResetLive.TerrainReady(id, out var reason)) return false;
      if (reason != null) return SkipLive(id, reason);
    }
    var zdo = ZDOMan.instance.GetZDO(id);
    if (zdo == null) return true;
    var canModify = handoff || areas.CanModify(zdo);
    if (!canModify)
    {
      if (!firstPass)
      {
        if (!live) return false;
        return SnowResetLive.Begin(zdo, areas) is { } reason && SkipLive(id, reason);
      }
      // A clean server copy can be missing edits the terrain owner has yet to send.
      if (live && CouldHaveUnsyncedSnow(zdo)) return false;
    }
    var compressed = zdo.GetByteArray(ZDOVars.s_TCData);
    if (compressed == null)
    {
      if (handoff) SnowResetLive.End(id);
      return true;
    }

    HeightmapBuilder.HMBuildData? natural = null;
    var center = zdo.GetPosition();
    var raw = Utils.Decompress(compressed);
    var result = SnowResetData.Refill(raw, NaturalSnow, out var points);
    if (result == null)
    {
      if (handoff) SnowResetLive.End(id);
      return true;
    }
    if (!canModify) return false;

    var payload = Utils.Compress(result);
    if (handoff)
    {
      if (SnowResetLive.Replace(zdo, compressed, payload) is { } reason) return SkipLive(id, reason);
    }
    else WriteTerrain(zdo, payload);
    changedRecords++;
    changedPoints += points;
    return !handoff;

    float? NaturalSnow(int x, int y, int width)
    {
      var scale = 64f / width;
      if (!IncludesPaint(PaintPosition(center, x, y, width, scale))) return null;
      natural ??= HeightmapBuilder.instance.RequestTerrainSync(center, width, scale, false, WorldGenerator.instance);
      return natural.m_baseMask[y * (width + 1) + x].g;
    }
  }

  private bool CouldHaveUnsyncedSnow(ZDO zdo)
  {
    var owner = zdo.GetOwner();
    if (owner == 0L || owner == session || network.GetPeer(owner) == null) return false;

    var center = zdo.GetPosition();
    var width = Zones.GetHeightmap(ZoneSystem.instance.m_zonePrefab).m_width;
    var scale = 64f / width;
    for (var y = 0; y <= width; y++)
      for (var x = 0; x <= width; x++)
        if (IncludesPaint(PaintPosition(center, x, y, width, scale))) return true;
    return false;
  }

  private bool IncludesPaint(Vector3 position) =>
    WorldGenerator.instance.GetBiome(position) == Heightmap.Biome.DeepNorth &&
    (!args.NoEdges || WorldGenerator.instance.GetBiomeArea(position) == Heightmap.BiomeArea.Median);

  private static Vector3 PaintPosition(Vector3 center, int x, int y, int width, float scale) =>
    center + new Vector3((x - width / 2f - 0.5f) * scale, 0f, (y - width / 2f - 0.5f) * scale);

  private bool SkipLive(ZDOID id, string reason)
  {
    var terrain = ZDOMan.instance.GetZDO(id);
    var zone = terrain == null ? (Vector2s?)null : ZoneSystem.GetZone(terrain.GetPosition());
    var where = zone is { } selected ? $"zone {selected.x},{selected.y}" : $"terrain record {id}";
    var message = $"Skipped live snow refill in {where}. {reason}.";
    UpgradeWorld.Log.LogWarning(message);
    skipMessage = message;
    skipped++;
    return true;
  }

  private void WriteTerrain(ZDO zdo, byte[] payload)
  {
    var owner = zdo.GetOwner();
    var restoreOwner = owner != 0L && owner != session;
    try
    {
      // FGN can treat a write on an orphaned record as a pending owner override.
      if (restoreOwner) zdo.SetOwnerInternal(session);
      zdo.Set(ZDOVars.s_TCData, payload);
    }
    finally
    {
      if (restoreOwner) zdo.SetOwnerInternal(owner);
    }
    RefreshCompiler(zdo);
  }

  private static void RefreshCompiler(ZDO zdo)
  {
    var scene = ZNetScene.instance;
    var instance = scene ? scene.FindInstance(zdo.m_uid) : null;
    var compiler = instance ? instance.GetComponent<TerrainComp>() : null;
    // Registered compilers have initialized the paint arrays.
    if (compiler && TerrainComp.FindTerrainCompiler(zdo.GetPosition()) == compiler) ReloadCompiler(compiler);
  }

  private static void ReloadCompiler(TerrainComp compiler)
  {
    checkLoad ??= AccessTools.MethodDelegate<Action<TerrainComp>>(AccessTools.Method(typeof(TerrainComp), "CheckLoad"));
    paintHash ??= AccessTools.MethodDelegate<Func<TerrainComp, int>>(AccessTools.Method(typeof(TerrainComp), "ComputePaintMaskHash"));
    lastHash ??= AccessTools.Field(typeof(TerrainComp), "m_lastHash");
    checkLoad(compiler);
    // Paint-only saves compare against this hash.
    lastHash.SetValue(compiler, paintHash(compiler));
  }

  protected override void OnEnd()
  {
    if (changedRecords > 0) Print($"Refilled {changedPoints} snow points in {changedRecords} terrain records");
    else if (skipped == 0) Print("No eligible Deep North snow to refill");
    if (skipped > 0)
    {
      Print(skipped == 1 && total == 1 ? skipMessage! : $"Skipped {skipped} terrain records during live sync. Check the server log for the affected zones.");
      Print("Add delayed to the same command to refill those areas when their terrain is clear");
    }
    if (unconfirmed > 0) Print($"Client refresh could not be confirmed for {unconfirmed} terrain records");
  }
}
