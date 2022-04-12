using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public static class Helper {
  public static string Normalize(string value) => value.Trim().ToLower();
  public static string JoinRows(IEnumerable<string> values) => string.Join(", ", values);

  public static List<string> AvailableBiomes = new(){
      "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains", "Swamp"
    };
  /// <summary>Converts a biome name to a biome.</summary>
  public static Heightmap.Biome GetBiome(string name) {
    name = Helper.Normalize(name);
    if (name == "ashlands") return Heightmap.Biome.AshLands;
    if (name == "blackforest") return Heightmap.Biome.BlackForest;
    if (name == "deepnorth") return Heightmap.Biome.DeepNorth;
    if (name == "meadows") return Heightmap.Biome.Meadows;
    if (name == "mistlands") return Heightmap.Biome.Mistlands;
    if (name == "mountain") return Heightmap.Biome.Mountain;
    if (name == "ocean") return Heightmap.Biome.Ocean;
    if (name == "plains") return Heightmap.Biome.Plains;
    if (name == "swamp") return Heightmap.Biome.Swamp;
    return Heightmap.Biome.None;
  }

  public static void RemoveZDO(ZDO zdo) {
    if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid) return;
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
    foreach (var zdo in sectorObjects) {
      var zdoPosition = zdo.GetPosition();
      var delta = position - zdoPosition;
      delta.y = 0;
      if (zdoPosition.y > 4000f || delta.magnitude < distance) Helper.RemoveZDO(zdo);
    }
  }
  /// <summary>Wraps the local player position for safe use..</summary>
  public static Vector3 GetLocalPosition() {
    if (Player.m_localPlayer) return Player.m_localPlayer.transform.position;
    return Vector3.zero;
  }
  public static Vector2i GetLocalZone() => ZoneSystem.instance.GetZone(GetLocalPosition());
  public static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0) {
    if (extra.Count() > handled) {
      args.Context.AddString("Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
      return false;
    }
    return true;
  }
  public static bool IsServer(Terminal.ConsoleEventArgs args) {
    var isServer = ZNet.instance && ZNet.instance.IsServer();
    if (!isServer) {
      ServerExecution.Send(args);
      return false;
    }
    var isDedicated = ZNet.instance && ZNet.instance.IsDedicated();
    if (isDedicated && RedirectOutput.Target == null) {
      args.Context.AddString("Error: Dedicated server is not allowed to execute commands.");
      return false;
    }
    return true;
  }
}
