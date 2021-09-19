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
    private static Vector2i[] GetZones(string operation) {
      var zones = Zones.GetAllZones();
      var generatedZones = zones.Length;
      zones = Filter.FilterByBiomes(zones, Settings.IncludedBiomes, Settings.ExcludedBiomes);
      var filteredByBiome = generatedZones - zones.Length;
      var filterPoints = Settings.GetFilterPoints(GetPlayerPosition());
      foreach (var filterPoint in filterPoints) {
        zones = Filter.FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
      }
      var filteredByPoints = generatedZones - filteredByBiome - zones.Length;
      var print = zones.Length + " zones to " + operation + " (from " + generatedZones + " generated zones " + filteredByBiome + " filtered by biome and " + filteredByPoints + " filtered by position)";
      Console.instance.Print(print);
      return zones;
    }
    public static bool Prefix(string text) {
      if (!ZNet.instance) {
        Console.instance.Print("Commands are not available. Please load a world first.");
        return true;
      }
      if (!ZNet.instance.IsServer()) {
        Console.instance.Print("Commands are not available for clients. Please load this world on a single player mode.");
        return true;
      }
      var array = text.Split(' ');
      if (array[0] == "upgrade") {
        Operation.SetOperation("upgrade_init", GetZones("upgrade"));
        return false;
      }
      if (array[0] == "nuke") {
        Operation.SetOperation("nuke", GetZones("nuke"));
        return false;
      }
      if (array[0] == "query") {
        _ = GetZones("operate");
        return false;
      }
      if (array[0] == "stop") {
        Operation.SetOperation("", new Vector2i[0]);
        return false;
      }
      if (array[0] == "genloc") {
        ZoneSystem.instance.GenerateLocations();
        return false;
      }
      return true;
    }
  }
}