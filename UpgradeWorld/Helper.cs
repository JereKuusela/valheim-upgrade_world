using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public static class Helper {
  public static string Normalize(string value) => value.Trim().ToLower();
  public static string JoinRows(IEnumerable<string> values) => string.Join(", ", values);

  /// <summary>Converts a biome name to a biome.</summary>
  public static Heightmap.Biome GetBiome(string name) {
    name = Helper.Normalize(name);
    if (Enum.TryParse<Heightmap.Biome>(name, true, out var biome))
      return biome;
    return Heightmap.Biome.None;
  }

  public static void RemoveZDO(ZDO zdo) {
    if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid) return;
    if (ZNet.instance.m_peers.Any(peer => peer.m_characterID == zdo.m_uid)) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }

  /// <summary>Clears the area around the location to prevent overlapping entities.</summary>
  public static void ClearAreaForLocation(Vector2i zone, ZoneSystem.LocationInstance location, bool force = false) {
    if (location.m_location.m_location.m_clearArea || force)
      ClearZDOsWithinDistance(zone, location.m_position, location.m_location.m_exteriorRadius);
  }

  /// <summary>Clears entities too close to a given position.</summary>
  public static void ClearZDOsWithinDistance(Vector2i zone, Vector3 position, float distance) {
    List<ZDO> sectorObjects = new();
    ZDOMan.instance.FindObjects(zone, sectorObjects);
    var scene = ZNetScene.instance;
    foreach (var zdo in sectorObjects) {
      var prefab = scene.GetPrefab(zdo.GetPrefab());
      if (prefab && prefab.name == Settings.ZoneControlId) continue;
      var zdoPosition = zdo.GetPosition();
      var delta = position - zdoPosition;
      delta.y = 0;
      if (zdoPosition.y > 4000f || delta.magnitude < distance) Helper.RemoveZDO(zdo);
    }
  }
  /// <summary>Returns the player's position while also handling the server-side.</summary>
  public static Vector3 GetPlayerPosition() {
    if (Player.m_localPlayer) return Player.m_localPlayer.transform.position;
    if (ServerExecution.User != null) {
      var player = ZNet.instance.m_peers.Find(peer => peer.IsReady() && peer.m_socket.GetHostName() == ServerExecution.User.GetSocket().GetHostName());
      if (player != null) return player.m_refPos;
    }
    return Vector3.zero;
  }
  /// <summary>Returns the player's zone while also handling the server-side.</summary>
  public static Vector2i GetPlayerZone() => ZoneSystem.instance.GetZone(GetPlayerPosition());
  public static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0) {
    if (extra.Count() > handled) {
      Helper.Print(args.Context, "Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
      return false;
    }
    return true;
  }
  public static bool IsClient(Terminal.ConsoleEventArgs args) {
    var isServer = ZNet.instance && ZNet.instance.IsServer();
    if (!isServer) {
      ServerExecution.Send(args);
      return true;
    }
    var isDedicated = ZNet.instance && ZNet.instance.IsDedicated();
    if (!Settings.DedicatedServerExecution && isDedicated && ServerExecution.User == null) {
      Helper.Print(args.Context, "Error: Dedicated server is not allowed to directly execute any commands (can be configured).");
      return true;
    }
    return false;
  }
  public static void Print(Terminal terminal, ZRpc? user, string value) {
    if (ZNet.m_isServer && user != null) {
      ZNet.instance.RemotePrint(user, value);
    }
    if (terminal) terminal.AddString(value);
  }
  public static void Print(Terminal terminal, string value) => Print(terminal, ServerExecution.User, value);

  private static string Previous = "";
  public static void PrintOnce(Terminal terminal, ZRpc? user, string value) {
    if (ZNet.m_isServer && user != null)
      user.Invoke(ServerExecution.RPC_RemotePrintOnce, value);
    if (!terminal) return;
    if (terminal.m_chatBuffer.LastOrDefault() == value) return;
    if (Previous != "")
      while (terminal.m_chatBuffer.Remove(Previous)) ;
    Previous = value;
    Print(terminal, null, value);
  }
  public static string IdString(IEnumerable<string> ids) {
    if (ids.Count() == 0) return "";
    return " " + Helper.JoinRows(ids);
  }
  public static string LocationIdString(IEnumerable<string> ids) {
    if (ids.Count() == ZoneSystem.instance.m_locations.Count) return "";
    if (ids.Count() == 0) return "";
    return " " + Helper.JoinRows(ids);
  }
}
