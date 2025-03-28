using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;
namespace UpgradeWorld;
public static class Helper
{
  public static bool IsValid(ZoneSystem.ZoneLocation loc) => loc != null && loc.m_prefab != null && (loc.m_prefab.IsValid || loc.m_prefab.m_name != null);
  public static string Normalize(string value) => value.Trim().ToLower();
  public static string JoinRows(IEnumerable<string> values) => string.Join(", ", values);

  /// <summary>Converts a biome name to a biome.</summary>
  public static Heightmap.Biome GetBiome(string name)
  {
    name = Normalize(name);
    if (Enum.TryParse<Heightmap.Biome>(name, true, out var biome))
      return biome;
    return Heightmap.Biome.None;
  }
  public static List<ZDO>? GetZDOs(Vector2i zone)
  {
    var zman = ZDOMan.instance;
    var index = zman.SectorToIndex(zone);
    if (index >= 0 && index < zman.m_objectsBySector.Length)
      return zman.m_objectsBySector[index];
    if (zman.m_objectsByOutsideSector.TryGetValue(zone, out var list))
      return list;
    return null;
  }
  public static void RemoveZDO(ZDO zdo)
  {
    if (zdo == null || !zdo.IsValid()) return;
    if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid) return;
    if (ZNet.instance.m_peers.Any(peer => peer.m_characterID == zdo.m_uid)) return;
    zdo.SetOwner(ZDOMan.GetSessionID());
    var spawned = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
    if (spawned != ZDOID.None && ZDOMan.instance.m_objectsByID.TryGetValue(spawned, out var spawnedZdo) && spawnedZdo != zdo)
      RemoveZDO(spawnedZdo);
    if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
      ZNetScene.instance.Destroy(view.gameObject);
    else
      ZDOMan.instance.DestroyZDO(zdo);
  }
  public static void MoveZDO(ZDO zdo, Vector3 pos, Quaternion rot)
  {
    ZDOData data = new(zdo);
    data.Move(pos, rot);
    RemoveZDO(zdo);
  }

  /// <summary>Clears the area around the location to prevent overlapping entities.</summary>
  public static void ClearAreaForLocation(Vector2i zone, ZoneSystem.LocationInstance location, float radius)
  {
    if (radius > 0f)
      ClearZDOsWithinDistance(zone, location.m_position, radius);
  }

  /// <summary>Clears entities too close to a given position.</summary>
  public static void ClearZDOsWithinDistance(Vector2i zone, Vector3 center, float distance)
  {
    if (distance == 0f) return;
    var sectorObjects = GetZDOs(zone);
    if (sectorObjects == null) return;
    foreach (var zdo in sectorObjects)
    {
      if (zdo.m_prefab == Settings.ZoneControlHash || zdo.m_prefab == Settings.TerrainCompilerHash) continue;
      var pos = zdo.GetPosition();
      var delta = center - pos;
      delta.y = 0;
      if (pos.y > 4000f || delta.magnitude < distance) RemoveZDO(zdo);
    }
  }
  /// <summary>Returns the player's position while also handling the server-side.</summary>
  public static Vector3 GetPlayerPosition()
  {
    if (Player.m_localPlayer) return Player.m_localPlayer.transform.position;
    if (ServerExecution.User != null)
    {
      var player = ZNet.instance.m_peers.Find(peer => peer.IsReady() && peer.m_socket.GetHostName() == ServerExecution.User.GetSocket().GetHostName());
      if (player != null) return player.m_refPos;
    }
    return Vector3.zero;
  }
  /// <summary>Returns the player's zone while also handling the server-side.</summary>
  public static Vector2i GetPlayerZone() => ZoneSystem.GetZone(GetPlayerPosition());
  public static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0)
  {
    if (extra.Count() > handled)
    {
      Print(args.Context, "Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
      return false;
    }
    return true;
  }
  public static bool IsClient(Terminal.ConsoleEventArgs args)
  {
    var isServer = ZNet.instance && ZNet.instance.IsServer();
    if (!isServer)
    {
      ServerExecution.Send(args);
      return true;
    }
    var isDedicated = ZNet.instance && ZNet.instance.IsDedicated();
    if (isDedicated && ServerExecution.User == null && !Settings.IsRoot("-1"))
    {
      Print(args.Context, "Error: Dedicated server is not allowed to directly execute any commands (-1 is missing from root users).");
      return true;
    }
    return false;
  }
  public static void Print(Terminal terminal, ZRpc? user, string value)
  {
    if (ZNet.m_isServer && user != null)
    {
      ZNet.instance.RemotePrint(user, value);
    }
    if (terminal) terminal.AddString(value);
  }
  public static void Print(Terminal terminal, string value) => Print(terminal, ServerExecution.User, value);

  private static string Previous = "";
  private static DateTime PreviousTime = new(0);
  public static void PrintOnce(Terminal terminal, ZRpc? user, string value, float limiter = 0f)
  {
    if (ZNet.m_isServer && user != null)
    {
      var now = DateTime.Now;
      if (limiter == 0f || now.Subtract(PreviousTime).TotalSeconds > limiter)
      {
        user.Invoke(ServerExecution.RPC_RemotePrintOnce, value);
        PreviousTime = now;
      }
    }
    if (!terminal) return;
    if (terminal.m_chatBuffer.LastOrDefault() == value) return;
    if (Previous != "")
      while (terminal.m_chatBuffer.Remove(Previous)) ;
    Previous = value;
    Print(terminal, null, value);
  }
  public static string IdString(IEnumerable<string> ids)
  {
    if (ids.Count() == 0) return "";
    return " " + JoinRows(ids);
  }


  public static string PrintPin(Vector3 vector) => vector.x.ToString("F0") + ", " + vector.z.ToString("F0") + ", " + vector.y.ToString("F0");
  public static string PrintVectorXZY(Vector3 vector) => "(" + vector.x.ToString("F0") + ", " + vector.z.ToString("F0") + ", " + vector.y.ToString("F0") + ")";
  public static string PrintVectorYXZ(Vector3 vector) => "(" + vector.y.ToString("F0") + ", " + vector.x.ToString("F0") + ", " + vector.z.ToString("F0") + ")";
  public static string PrintAngleYXZ(Quaternion quaternion) => PrintVectorYXZ(quaternion.eulerAngles);

  public static string PrintDay(long v) => ToDay(v).ToString();

  public static long ToDay(long v) => v / 1000 / 10000 / 1800;
  public static long ToTick(long v) => v * 1000 * 10000 * 1800;

  public static void RecalculateTerrain()
  {
    foreach (var hm in Heightmap.s_heightmaps)
    {
      hm.m_buildData = null;
      hm.Poke(true);
    }
  }

  public static void Command(string name, string description, Terminal.ConsoleEvent action, Terminal.ConsoleOptionsFetcher? fetcter = null)
  {
    new Terminal.ConsoleCommand(name, description, Catch(action), isCheat: true, isNetwork: true, optionsFetcher: fetcter);
  }
  public static Terminal.ConsoleEvent Catch(Terminal.ConsoleEvent action) =>
    (args) =>
    {
      try
      {
        action(args);
      }
      catch (InvalidOperationException e)
      {
        AddError(args.Context, e.Message);
      }
    };
  public static void Command(string name, string description, Terminal.ConsoleEventFailable action, Terminal.ConsoleOptionsFetcher? fetcter = null)
  {
    new Terminal.ConsoleCommand(name, description, Catch(action), isCheat: true, isNetwork: true, optionsFetcher: fetcter);
  }
  public static Terminal.ConsoleEventFailable Catch(Terminal.ConsoleEventFailable action) =>
    (args) =>
    {
      try
      {
        return action(args);
      }
      catch (InvalidOperationException e)
      {
        AddError(args.Context, e.Message);
      }
      return null;
    };

  public static void AddError(Terminal context, string message, bool priority = false)
  {
    AddMessage(context, $"Error: {message}", priority);
  }
  public static void AddMessage(Terminal context, string message, bool priority = false)
  {
    if (context == Console.instance)
      context.AddString(message);
    var hud = MessageHud.instance;
    if (!hud) return;
    if (!Player.m_localPlayer) return;
    if (priority)
    {
      var items = hud.m_msgQeue.ToArray();
      hud.m_msgQeue.Clear();
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, message);
      foreach (var item in items)
        hud.m_msgQeue.Enqueue(item);
      hud.m_msgQueueTimer = 10f;
    }
    else
    {
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, message);
    }
  }
}
