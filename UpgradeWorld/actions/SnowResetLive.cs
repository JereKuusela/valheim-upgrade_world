using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;

[HarmonyPatch]
internal static class SnowResetLive
{
  private const float ReplyTimeout = 8f;

  private sealed class Handoff(ZDO original, Vector2s zone, ushort revision, List<ZNetPeer> observers)
  {
    internal readonly ZDO Original = original;
    internal readonly ZDOID Id = original.m_uid;
    internal readonly long FormerOwner = original.GetOwner();
    internal readonly ushort OwnerRevision = revision;
    internal readonly Vector2s Zone = zone;
    internal readonly Dictionary<ZRpc, uint> TerrainReplies = [];
    internal readonly Dictionary<ZNetPeer, int> SceneReplies = observers.ToDictionary(peer => peer, _ => -1);
    internal float Deadline = Time.realtimeSinceStartup + ReplyTimeout;
    internal float Ping;
    internal ZDOID Replacement;
  }

  private sealed class Retirement(uint revision, byte[] terrain)
  {
    internal readonly uint Revision = revision;
    internal readonly byte[] TerrainHash = HashTerrain(terrain);
    internal bool ReportedChanged;
  }

  private static readonly Dictionary<ZDOID, Retirement> retired = [];
  private static readonly Dictionary<ZDOID, Handoff> handoffs = [];
  private static readonly HashSet<ZDOID> replacements = [];
  private static ZDOMan? world;
  private static int nextPing;

  private static ZDO? FindTerrain(Vector2s zone)
  {
    ZDO? result = null;
    var records = Helper.GetZDOs(zone);
    if (records == null) return null;
    foreach (var record in records)
    {
      if (record.GetPrefab() != Settings.TerrainCompilerHash || ZoneSystem.GetZone(record.GetPosition()) != zone) continue;
      if (result != null) return null;
      result = record;
    }
    return result;
  }

  internal static string? Begin(ZDO original, SnowResetAreas areas)
  {
    var zone = ZoneSystem.GetZone(original.GetPosition());
    var manager = ZDOMan.instance;
    if (Validate(original, zone) is { } invalid) return invalid;
    if (areas.GetPeers() is not { } peers) return "A client's terrain state is unavailable";

    var observers = new List<ZNetPeer>();
    var known = new List<(ZNetPeer Peer, uint Revision)>();
    var ownerRevision = original.OwnerRevision;
    var owner = original.GetOwner();
    foreach (var (peer, terrain) in peers)
    {
      var near = peer.m_simulationDistance.NearSimulationDistance;
      if (near < 1) return "A client's terrain state is unavailable";
      if (terrain[original.m_uid] is { } previous)
      {
        if (SnowResetAreas.GetRevisions(previous) is not { } stamp)
          return "A client's terrain revision is unavailable";
        var revision = Math.Min(stamp.Data, original.DataRevision);
        if (revision == 0) return "A client terrain copy has no established revision";
        ownerRevision = Math.Max(ownerRevision, stamp.Owner);
        known.Add((peer, revision - 1));
      }
      else if (peer.m_uid == owner)
        return "The owner has no established terrain copy";
      else if (!SnowResetAreas.IsNear(zone, peer, near))
        continue;
      observers.Add(peer);
    }
    if (ownerRevision >= ushort.MaxValue - 1)
      return "The terrain owner revision is exhausted";

    world = manager;
    var active = new Handoff(original, zone, (ushort)(ownerRevision + 1), observers);
    handoffs.Add(active.Id, active);
    TakeOwnership(active);
    try
    {
      var saved = new ZPackage();
      original.Serialize(saved);
      foreach (var (peer, revision) in known)
      {
        active.TerrainReplies.Add(peer.m_rpc, revision);
        var update = new ZPackage();
        update.Write(0);
        update.Write(active.Id);
        update.Write(active.OwnerRevision);
        // An ownership-only update leaves the client's edits intact and makes RequestZDO reply.
        update.Write(revision);
        update.Write(ZDOMan.GetSessionID());
        update.Write(original.GetPosition());
        update.Write(saved);
        update.Write(ZDOID.None);
        peer.m_rpc.Invoke("ZDOData", update);
        ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "RequestZDO", active.Id);
      }
    }
    catch
    {
      End(active.Id);
      throw;
    }
    return null;
  }

  private static string? Validate(ZDO original, Vector2s zone)
  {
    if (FindTerrain(zone) != original)
      return "The zone no longer has the same single terrain compiler";
    var oldId = original.m_uid;
    if (ZNetScene.instance && ZNetScene.instance.FindInstance(oldId))
      return "The host also has this terrain loaded";
    if (!original.Persistent || ZDOExtraData.s_connections.ContainsKey(oldId) ||
        ZDOExtraData.s_connectionsHashData.ContainsKey(oldId) ||
        ZDOExtraData.s_connections.Values.Any(connection => connection.m_target == oldId))
      return "The terrain compiler must be persistent and have no object links";
    if (ZDOMan.instance.m_destroySendList.Contains(oldId))
      return "The terrain compiler is already scheduled for deletion";
    return null;
  }

  private static void TakeOwnership(Handoff active)
  {
    active.Original.SetOwnerInternal(ZDOMan.GetSessionID());
    active.Original.OwnerRevision = active.OwnerRevision;
  }

  internal static bool HasHandoff(ZDOID id, out bool replaced)
  {
    var found = handoffs.TryGetValue(id, out var active);
    replaced = active != null && !active.Replacement.IsNone();
    return found;
  }

  internal static bool TerrainReady(ZDOID id, out string? message)
  {
    var active = handoffs[id];
    message = null;
    if (ZDOMan.instance.GetZDO(active.Id) != active.Original || !HasTerrainLock(active))
    {
      End(id);
      message = "The terrain owner changed during sync";
      return true;
    }
    if (active.TerrainReplies.Count == 0) return true;
    if (Time.realtimeSinceStartup < active.Deadline &&
        active.TerrainReplies.Keys.All(rpc => ZNet.instance.GetPeer(rpc) is { } peer && peer.IsReady())) return false;
    End(id);
    message = "Client terrain sync did not complete";
    return true;
  }

  internal static string? Replace(ZDO original, byte[] before, byte[] refill)
  {
    var active = handoffs[original.m_uid];
    if (Validate(original, active.Zone) is { } invalid)
    {
      End(active.Id);
      return invalid;
    }

    var manager = ZDOMan.instance;
    var oldId = active.Id;
    var saved = new ZPackage();
    original.Serialize(saved);
    saved.SetPos(0);
    var revision = original.DataRevision;
    var replacement = manager.CreateNewZDO(original.GetPosition(), original.GetPrefab());
    var newId = replacement.m_uid;
    try
    {
      replacement.Deserialize(saved);
      replacement.Set(ZDOVars.s_TCData, refill);
      if (!replacement.Persistent || replacement.GetPrefab() != original.GetPrefab() ||
          replacement.GetByteArray(ZDOVars.s_TCData) is not { } staged || !staged.SequenceEqual(refill))
        throw new InvalidOperationException("The replacement terrain did not stage correctly");
      retired.Add(oldId, new(revision, before));

      var deletion = new ZPackage();
      deletion.Write(1);
      deletion.Write(oldId);
      ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DestroyZDO", deletion);
      if (manager.GetZDO(oldId) != null)
        throw new InvalidOperationException("The terrain retirement did not complete");
      active.Replacement = newId;
      replacements.Add(newId);
    }
    catch
    {
      // Keep the replacement if the original has already been retired.
      if (manager.GetZDO(oldId) != null)
      {
        retired.Remove(oldId);
        manager.HandleDestroyedZDO(newId);
      }
      else active.Replacement = newId;
      End(oldId);
      throw;
    }
    UpgradeWorld.Log.LogInfo($"Live snow zone {active.Zone.x},{active.Zone.y} retired {oldId} before publishing {newId} at terrain revision {revision}.");
    return null;
  }

  internal static void RequestSceneReplies()
  {
    if (replacements.Count == 0) return;
    HashSet<ZNetPeer>? waiting = null;
    var token = (float)(nextPing - 1);
    foreach (var active in handoffs.Values)
    {
      if (active.Ping != 0f || active.Replacement.IsNone()) continue;
      active.Ping = token;
      active.Deadline = Time.realtimeSinceStartup + ReplyTimeout;
      waiting ??= [];
      waiting.UnionWith(active.SceneReplies.Keys);
    }
    if (waiting == null) return;
    nextPing--;
    var connected = ZNet.instance.GetPeers();
    foreach (var peer in waiting)
      if (connected.Contains(peer)) ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "RPC_Ping", token);
  }

  internal static bool SceneReady(ZDOID id, out bool confirmed)
  {
    var active = handoffs[id];
    var connected = ZNet.instance.GetPeers();
    confirmed = !active.SceneReplies.Any(reply => reply.Value < 2 && connected.Contains(reply.Key));
    if (!confirmed && Time.realtimeSinceStartup < active.Deadline) return false;
    if (!confirmed) UpgradeWorld.Log.LogWarning($"Client terrain refresh confirmation timed out in zone {active.Zone.x},{active.Zone.y}.");
    End(id);
    return true;
  }

  private static bool HasTerrainLock(Handoff active) => active.Original.GetOwner() == ZDOMan.GetSessionID() &&
    active.Original.OwnerRevision == active.OwnerRevision;

  internal static void End()
  {
    foreach (var id in handoffs.Keys.ToArray()) End(id);
  }

  internal static void End(ZDOID id)
  {
    if (!handoffs.TryGetValue(id, out var active)) return;
    handoffs.Remove(id);
    replacements.Remove(active.Replacement);
    if (!ZNet.instance || world != ZDOMan.instance) return;
    var replacement = !active.Replacement.IsNone();
    var zdo = world!.GetZDO(replacement ? active.Replacement : active.Id);
    if (zdo == null || !replacement && (!ReferenceEquals(zdo, active.Original) || !HasTerrainLock(active))) return;
    // A second revision refreshes the new compiler after the old Unity object has exited its frame.
    if (replacement) zdo.IncreaseDataRevision();
    var owner = active.FormerOwner;
    var peer = ZNet.instance.GetPeer(owner);
    if (peer != null && peer.IsReady() && (!replacement || ZNetScene.InActiveArea(zdo.GetPosition(), peer.GetRefPos())))
      zdo.SetOwner(owner);
    else if (!replacement && owner != ZDOMan.GetSessionID()) zdo.SetOwner(0L);
  }

  [HarmonyPatch(typeof(ZDO), nameof(ZDO.SetOwner)), HarmonyPrefix]
  private static bool AllowOwnerChange(ZDO __instance) => handoffs.Count == 0 ||
    !handoffs.ContainsKey(__instance.m_uid) && !replacements.Contains(__instance.m_uid);

  [HarmonyPatch(typeof(ZDOMan), "CreateSyncList"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
  private static void HoldTerrain(List<ZDO> toSync)
  {
    // Normal sends would advance the client's reply cache before it can answer RequestZDO.
    if (handoffs.Count > replacements.Count)
      toSync.RemoveAll(zdo => handoffs.TryGetValue(zdo.m_uid, out var active) && active.Replacement.IsNone());
  }

  [HarmonyPatch(typeof(Game), "RPC_Pong"), HarmonyPrefix]
  private static bool ObservePong(long sender, float time)
  {
    if (time >= 0f || replacements.Count == 0 || ZNet.instance.GetPeer(sender) is not { } peer) return true;
    var observed = false;
    foreach (var active in handoffs.Values)
    {
      if (time != active.Ping || !active.SceneReplies.TryGetValue(peer, out var count)) continue;
      if (count == -1) active.SceneReplies[peer] = 0;
      observed = true;
    }
    return !observed;
  }

  [HarmonyPatch(typeof(ZNet), "RPC_ServerSyncedPlayerData"), HarmonyPostfix]
  private static void ObserveClientUpdate(ZRpc rpc)
  {
    // The first periodic packet can share the frame that handled the deletion.
    if (replacements.Count == 0 || ZNet.instance.GetPeer(rpc) is not { } peer) return;
    foreach (var active in handoffs.Values)
      if (active.SceneReplies.TryGetValue(peer, out var count) && count is 0 or 1)
        active.SceneReplies[peer] = count + 1;
  }

  [HarmonyPatch(typeof(ZDOMan), "RPC_ZDOData"), HarmonyPrefix]
  private static void RememberPacket(ZDOMan __instance, ZPackage pkg, out int __state) =>
    __state = handoffs.Count > replacements.Count && __instance == world ? pkg.GetPos() : -1;

  [HarmonyPatch(typeof(ZDOMan), "RPC_ZDOData"), HarmonyPostfix, HarmonyPriority(Priority.Last)]
  private static void ReceiveTerrain(ZDOMan __instance, ZRpc rpc, ZPackage pkg, int __state)
  {
    if (__instance != world) return;
    if (__state >= 0) ObserveTerrainReplies(__instance, rpc, pkg, __state);
    if (retired.Count == 0) return;
    foreach (var id in __instance.m_destroySendList)
    {
      if (!retired.TryGetValue(id, out var previous) || __instance.GetZDO(id) is not { } late) continue;
      if (!previous.ReportedChanged && late.DataRevision > previous.Revision &&
          late.GetByteArray(ZDOVars.s_TCData) is { } terrain && !HashTerrain(terrain).SequenceEqual(previous.TerrainHash))
      {
        previous.ReportedChanged = true;
        UpgradeWorld.Log.LogWarning($"Live snow handoff received changed terrain for retired record {id} at revision {late.DataRevision}. Treat this test as failed.");
      }
      // Vanilla queues a second destroy after accepting a late update for a retired ID.
      __instance.HandleDestroyedZDO(id);
    }
  }

  private static void ObserveTerrainReplies(ZDOMan manager, ZRpc rpc, ZPackage pkg, int start)
  {
    var end = pkg.GetPos();
    try
    {
      pkg.SetPos(start);
      for (var count = pkg.ReadInt(); count > 0; count--) pkg.ReadZDOID();
      while (pkg.GetPos() < end)
      {
        var id = pkg.ReadZDOID();
        if (id.IsNone()) break;
        var ownerRevision = pkg.ReadUShort();
        var dataRevision = pkg.ReadUInt();
        var owner = pkg.ReadLong();
        if (handoffs.TryGetValue(id, out var active) && active.Replacement.IsNone() && manager.GetZDO(id) == active.Original)
        {
          if (active.Original.OwnerRevision < active.OwnerRevision) TakeOwnership(active);
          if (ownerRevision == active.OwnerRevision && owner == ZDOMan.GetSessionID() &&
              active.TerrainReplies.TryGetValue(rpc, out var advertised) && dataRevision > advertised)
          {
            var formerOwner = ZNet.instance.GetPeer(rpc)?.m_uid == active.FormerOwner;
            if (!formerOwner || dataRevision >= active.Original.DataRevision) active.TerrainReplies.Remove(rpc);
          }
        }
        pkg.ReadVector3();
        var size = pkg.ReadInt();
        pkg.SetPos(pkg.GetPos() + size);
      }
    }
    finally { pkg.SetPos(end); }
  }

  private static byte[] HashTerrain(byte[] terrain)
  {
    using var hash = SHA256.Create();
    return hash.ComputeHash(terrain);
  }

  internal static void Clear()
  {
    handoffs.Clear();
    replacements.Clear();
    retired.Clear();
    world = null;
  }
}
