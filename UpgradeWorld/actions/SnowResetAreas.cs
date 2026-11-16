using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UpgradeWorld;

internal sealed class SnowResetAreas
{
  private static Func<ZDOMan, ZNetPeer, object?>? findPeer;
  private static FieldInfo? knownField, dataRevisionField, ownerRevisionField;
  private static bool inspected;
  private int frame = -1, peerCount;
  private bool blocked, warned;
  private readonly List<(ZNetPeer Peer, IDictionary Known)> peers = [];

  internal bool CanModify(ZDO zdo)
  {
    var ready = GetPeers();
    if (ready == null) return false;
    if (ready.Count == 0) return true;
    var zone = ZoneSystem.GetZone(zdo.GetPosition());
    var owner = zdo.GetOwner();
    foreach (var (peer, known) in ready)
    {
      var near = peer.m_simulationDistance.NearSimulationDistance;
      if (near < 1) return false;
      if (owner == peer.m_uid || known.Contains(zdo.m_uid)) return false;
      if (IsNear(zone, peer, near)) return false;
    }
    return true;
  }

  internal List<(ZNetPeer Peer, IDictionary Known)>? GetPeers()
  {
    var current = ZNet.instance.GetPeers();
    if (frame != Time.frameCount || current.Count != peerCount) Refresh(current);
    return blocked ? null : peers;
  }

  // Adjacent zones can share a terrain border.
  internal static bool IsNear(Vector2s zone, ZNetPeer peer, int near) =>
    Zones.Distance(zone, ZoneSystem.GetZone(peer.GetRefPos())) <= near + 1;

  internal static (uint Data, ushort Owner)? GetRevisions(object entry)
  {
    if (dataRevisionField?.GetValue(entry) is uint data && ownerRevisionField?.GetValue(entry) is ushort owner)
      return (data, owner);
    return null;
  }

  private void Refresh(List<ZNetPeer> current)
  {
    frame = Time.frameCount;
    peerCount = current.Count;
    peers.Clear();
    blocked = false;
    if (current.Count == 0) return;
    try
    {
      if (!inspected)
      {
        inspected = true;
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var lookup = typeof(ZDOMan).GetMethod("FindPeer", fields, null, [typeof(ZNetPeer)], null);
        knownField = lookup?.ReturnType.GetField("m_zdos", fields);
        findPeer = (Func<ZDOMan, ZNetPeer, object?>?)lookup?.CreateDelegate(typeof(Func<ZDOMan, ZNetPeer, object>));
        if (knownField?.FieldType.GetGenericArguments() is [_, var entry])
        {
          dataRevisionField = entry.GetField("m_dataRevision", fields);
          ownerRevisionField = entry.GetField("m_ownerRevision", fields);
        }
      }
      if (findPeer == null || knownField == null) throw new MissingMemberException("ZDO peer state is unavailable");
      foreach (var peer in current)
      {
        if (!peer.IsReady()) { blocked = true; return; }
        var nativePeer = findPeer(ZDOMan.instance, peer);
        if (nativePeer == null || knownField.GetValue(nativePeer) is not IDictionary known)
          throw new InvalidOperationException("ZDO peer is not initialized");
        peers.Add((peer, known));
      }
    }
    catch (Exception)
    {
      blocked = true;
      if (warned) return;
      warned = true;
      UpgradeWorld.Log.LogWarning("Snow reset is waiting for client terrain state.");
    }
  }
}
