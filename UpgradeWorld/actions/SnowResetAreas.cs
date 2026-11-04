using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UpgradeWorld;

internal sealed class SnowResetAreas
{
  private static FieldInfo? peerList, peerField, knownField;
  private static bool inspected;
  private int frame = -1, peerCount;
  private bool blocked, warned;
  private readonly List<(ZNetPeer Peer, IDictionary Known)> peers = [];

  internal bool CanModify(ZDO zdo)
  {
    var current = ZNet.instance.GetPeers();
    if (frame != Time.frameCount || current.Count != peerCount) Refresh(current);
    if (blocked) return false;
    var zone = ZoneSystem.GetZone(zdo.GetPosition());
    foreach (var (peer, known) in peers)
    {
      var near = peer.m_simulationDistance.NearSimulationDistance;
      if (near < 1) return false;
      if (zdo.GetOwner() == peer.m_uid || known.Contains(zdo.m_uid)) return false;
      // Read the current negotiated value; settings can change while this job waits.
      if (Zones.Distance(zone, ZoneSystem.GetZone(peer.GetRefPos())) <= near + 1) return false;
    }
    return true;
  }

  private void Refresh(List<ZNetPeer> current)
  {
    frame = Time.frameCount;
    peerCount = current.Count;
    peers.Clear();
    blocked = false;
    if (current.Count == 0) return;
    if (current.Any(peer => !peer.IsReady())) { blocked = true; return; }
    try
    {
      if (!inspected)
      {
        inspected = true;
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        peerList = typeof(ZDOMan).GetField("m_peers", fields);
        var type = peerList?.FieldType.GetGenericArguments().Single();
        peerField = type?.GetField("m_peer", fields);
        knownField = type?.GetField("m_zdos", fields);
      }
      if (peerList == null || peerField == null || knownField == null)
        throw new MissingFieldException("ZDO peer state is unavailable");
      var native = peerList.GetValue(ZDOMan.instance) as IEnumerable
        ?? throw new InvalidOperationException("ZDO peers are unavailable");
      foreach (var peer in current)
      {
        IDictionary? known = null;
        foreach (var entry in native)
          if (ReferenceEquals(peerField.GetValue(entry), peer))
          {
            known = knownField.GetValue(entry) as IDictionary;
            break;
          }
        if (known == null) throw new InvalidOperationException("ZDO peer is not initialized");
        peers.Add((peer, known));
      }
    }
    catch (Exception)
    {
      blocked = true;
      if (warned) return;
      warned = true;
      UpgradeWorld.Log.LogWarning("Cannot verify client terrain caches. Snow reset will wait for connected players to leave");
    }
  }
}
