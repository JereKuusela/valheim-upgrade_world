using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld {

  // Replace devcommands check with a custom one.
  [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
  public class TryRunCommand {
    private static Vector3 GetPlayerPosition() {
      var player = Player.m_localPlayer;
      return player ? player.transform.position : new Vector3(0, 0, 0);
    }
    private static Vector2i[] GetZones() {
      var zones = Zones.GetAllZones();
      var generatedZones = zones.Length;
      zones = Filter.FilterByBiomes(zones, Settings.IncludedBiomes, Settings.ExcludedBiomes);
      var filteredByBiome = generatedZones - zones.Length;
      var filterPoints = Settings.GetFilterPoints(GetPlayerPosition());
      foreach (var filterPoint in filterPoints) {
        zones = Filter.FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
      }
      var filteredByPoints = generatedZones - filteredByBiome - zones.Length;
      var print = zones.Length + " zones to upgrade (from " + generatedZones + " generated zones " + filteredByBiome + " filtered by biome and " + filteredByPoints + " filtered by position)";
      Console.instance.Print(print);
      return zones;
    }
    public static bool Prefix(string text) {
      if (!ZNet.instance.IsServer()) {
        return true;
      }
      var array = text.Split(' ');
      if (array[0] == "upgrade") {
        Operation.SetOperation("upgrade", GetZones());
        return false;
      }
      if (array[0] == "nuke") {
        Operation.SetOperation("nuke", GetZones());
        return false;
      }
      if (array[0] == "query") {
        _ = GetZones();
        return false;
      }
      if (array[0] == "stop") {
        Operation.SetOperation("", new Vector2i[0]);
        return false;
      }
      return true;
    }
  }
}