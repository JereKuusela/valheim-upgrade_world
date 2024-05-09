using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Service;

// The most reliable way to edit objects is simply respawning them with the new data.
public class ZDOData
{
  public ZDOData(ZDO zdo)
  {
    Zdo = zdo;
    Load(zdo);
  }

  public ZDO Clone()
  {
    var zdo = ZDOMan.instance.CreateNewZDO(Zdo.m_position, 0);
    zdo.Persistent = Zdo.Persistent;
    zdo.Type = Zdo.Type;
    zdo.Distant = Zdo.Distant;
    zdo.m_prefab = Prefab;
    zdo.m_rotation = Zdo.m_rotation;
    zdo.SetOwnerInternal(Zdo.GetOwner());
    Write(zdo);
    return zdo;
  }
  public ZDO Move(Vector3 pos, Quaternion rot)
  {
    var zdo = ZDOMan.instance.CreateNewZDO(pos, 0);
    zdo.Persistent = Zdo.Persistent;
    zdo.Type = Zdo.Type;
    zdo.Distant = Zdo.Distant;
    zdo.m_prefab = Prefab;
    zdo.m_rotation = rot.eulerAngles;
    zdo.SetOwnerInternal(Zdo.GetOwner());
    Write(zdo);
    return zdo;
  }

  public ZDO Zdo;
  public int Prefab = 0;
  public Dictionary<int, string> Strings = [];
  public Dictionary<int, float> Floats = [];
  public Dictionary<int, int> Ints = [];
  public Dictionary<int, long> Longs = [];
  public Dictionary<int, Vector3> Vecs = [];
  public Dictionary<int, Quaternion> Quats = [];
  public Dictionary<int, byte[]> ByteArrays = [];
  public ZDOExtraData.ConnectionType ConnectionType = ZDOExtraData.ConnectionType.None;
  public int ConnectionHash = 0;
  public ZDOID OriginalId = ZDOID.None;
  public ZDOID TargetConnectionId = ZDOID.None;

  private void Write(ZDO zdo)
  {
    var id = zdo.m_uid;
    if (Floats.Count > 0) ZDOHelper.Init(ZDOExtraData.s_floats, id);
    if (Vecs.Count > 0) ZDOHelper.Init(ZDOExtraData.s_vec3, id);
    if (Quats.Count > 0) ZDOHelper.Init(ZDOExtraData.s_quats, id);
    if (Ints.Count > 0) ZDOHelper.Init(ZDOExtraData.s_ints, id);
    if (Longs.Count > 0) ZDOHelper.Init(ZDOExtraData.s_longs, id);
    if (Strings.Count > 0) ZDOHelper.Init(ZDOExtraData.s_strings, id);
    if (ByteArrays.Count > 0) ZDOHelper.Init(ZDOExtraData.s_byteArrays, id);

    foreach (var pair in Floats)
      ZDOExtraData.s_floats[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in Vecs)
      ZDOExtraData.s_vec3[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in Quats)
      ZDOExtraData.s_quats[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in Ints)
      ZDOExtraData.s_ints[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in Longs)
      ZDOExtraData.s_longs[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in Strings)
      ZDOExtraData.s_strings[id].SetValue(pair.Key, pair.Value);
    foreach (var pair in ByteArrays)
      ZDOExtraData.s_byteArrays[id].SetValue(pair.Key, pair.Value);

    HandleConnection(zdo);
    HandleHashConnection(zdo);
  }
  private void HandleConnection(ZDO ownZdo)
  {
    if (OriginalId == ZDOID.None) return;
    var ownId = ownZdo.m_uid;
    if (TargetConnectionId != ZDOID.None)
    {
      // If target is known, the setup is easy.
      var otherZdo = ZDOMan.instance.GetZDO(TargetConnectionId);
      if (otherZdo == null) return;

      ownZdo.SetConnection(ConnectionType, TargetConnectionId);
      // Portal is two way.
      if (ConnectionType == ZDOExtraData.ConnectionType.Portal)
        otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);

    }
    else
    {
      // Otherwise all zdos must be scanned.
      var other = ZDOExtraData.s_connections.FirstOrDefault(kvp => kvp.Value.m_target == OriginalId);
      if (other.Value == null) return;
      var otherZdo = ZDOMan.instance.GetZDO(other.Key);
      if (otherZdo == null) return;
      // Connection is always one way here, otherwise TargetConnectionId would be set.
      otherZdo.SetConnection(other.Value.m_type, ownId);
    }
  }
  private void HandleHashConnection(ZDO ownZdo)
  {
    if (ConnectionHash == 0) return;
    if (ConnectionType == ZDOExtraData.ConnectionType.None) return;
    var ownId = ownZdo.m_uid;

    // Hash data is regenerated on world save.
    // But in this case, it's manually set, so might be needed later.
    ZDOExtraData.SetConnectionData(ownId, ConnectionType, ConnectionHash);

    // While actual connection can be one way, hash is always two way.
    // One of the hashes always has the target type.
    var otherType = ConnectionType ^ ZDOExtraData.ConnectionType.Target;
    var isOtherTarget = (ConnectionType & ZDOExtraData.ConnectionType.Target) == 0;
    var zdos = ZDOExtraData.GetAllConnectionZDOIDs(otherType);
    var otherId = zdos.FirstOrDefault(z => ZDOExtraData.GetConnectionHashData(z, ConnectionType)?.m_hash == ConnectionHash);
    if (otherId == ZDOID.None) return;
    var otherZdo = ZDOMan.instance.GetZDO(otherId);
    if (otherZdo == null) return;
    if ((ConnectionType & ZDOExtraData.ConnectionType.Spawned) > 0)
    {
      // Spawn is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.Spawned, targetId);
    }
    if ((ConnectionType & ZDOExtraData.ConnectionType.SyncTransform) > 0)
    {
      // Sync is one way.
      var connZDO = isOtherTarget ? ownZdo : otherZdo;
      var targetId = isOtherTarget ? otherId : ownId;
      connZDO.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, targetId);
    }
    if ((ConnectionType & ZDOExtraData.ConnectionType.Portal) > 0)
    {
      // Portal is two way.
      otherZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ownId);
      ownZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, otherId);
    }
  }
  private void Load(ZDO zdo)
  {
    var id = zdo.m_uid;
    Prefab = zdo.m_prefab;
    Floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    Quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    ByteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    if (ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var conn))
    {
      ConnectionType = conn.m_type;
      ConnectionHash = conn.m_hash;
    }
    OriginalId = id;
    if (ZDOExtraData.s_connections.TryGetValue(id, out var zdoConn) && zdoConn.m_target != ZDOID.None)
    {
      TargetConnectionId = zdoConn.m_target;
      ConnectionType = zdoConn.m_type;
    }
  }
}