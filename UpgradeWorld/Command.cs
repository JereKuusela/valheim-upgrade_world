using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
    private static Vector3 GetPlayerPosition() {
      var player = Player.m_localPlayer;
      return player ? player.transform.position : new Vector3(0, 0, 0);
    }
    private static Vector2i[] GetZones(Terminal context, string operation) {
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
      context.AddString(print);
      return zones;
    }
    public static void Init() {
      new Terminal.ConsoleCommand("upgrade", "upgrades zones with new content", delegate (Terminal.ConsoleEventArgs args) {
        Operation.SetOperation(args.Context, "upgrade_init", GetZones(args.Context, "upgrade"));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("nuke", "destroys zones", delegate (Terminal.ConsoleEventArgs args) {
        Operation.SetOperation(args.Context, "nuke", GetZones(args.Context, "nuke"));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("query", "returns how many zones would get operated with current config", delegate (Terminal.ConsoleEventArgs args) {
        _ = GetZones(args.Context, "operate");
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "stops execution of current operation", delegate (Terminal.ConsoleEventArgs args) {
        Operation.SetOperation(args.Context, "", new Vector2i[0]);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("place", "runs genloc without needing cheats", delegate (Terminal.ConsoleEventArgs args) {
        ZoneSystem.instance.GenerateLocations();
      }, onlyServer: true);
    }
  }
}