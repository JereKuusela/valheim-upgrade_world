using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
    private static Vector3 GetPlayerPosition() {
      var player = Player.m_localPlayer;
      return player ? player.transform.position : new Vector3(0, 0, 0);
    }
    private static Vector2i[] GetZones(Terminal context, Operation operation) {
      var zones = operation == Operation.Generate ? Zones.GetWorldZones() : Zones.GetAllZones();
      var generatedZones = zones.Length;
      zones = Filter.FilterByBiomes(zones, Settings.IncludedBiomes, Settings.ExcludedBiomes);
      var filteredByBiome = generatedZones - zones.Length;
      var filterPoints = Settings.GetFilterPoints(GetPlayerPosition());
      foreach (var filterPoint in filterPoints) {
        zones = Filter.FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
      }
      var filteredByPoints = generatedZones - filteredByBiome - zones.Length;
      var print = zones.Length + " zones to " + Operations.GetName(operation) + " (from " + generatedZones + " generated zones " + filteredByBiome + " filtered by biome and " + filteredByPoints + " filtered by position)";
      context.AddString(print);
      return zones;
    }
    private static Vector2i[] GetAdjacentZones(Terminal context, Vector2i center, int adjacent, Operation operation) {
      var zones = Filter.FilterByAdjacent(Zones.GetAllZones(), center, adjacent);
      var print = zones.Length + " zones to " + Operations.GetName(operation);
      context.AddString(print);
      return zones;
    }
    private static Vector2i[] GetIncludedZones(Terminal context, Vector3 center, float radius, Operation operation) {
      var zones = Filter.FilterByRadius(Zones.GetAllZones(), center, radius);
      var print = zones.Length + " zones to " + Operations.GetName(operation);
      context.AddString(print);
      return zones;
    }
    public static void Init() {
      new Terminal.ConsoleCommand("upgrade", "- Upgrades all zones defined by the configuration.", delegate (Terminal.ConsoleEventArgs args) {
        Operations.SetOperation(args.Context, Operation.UpgradeInit, GetZones(args.Context, Operation.Upgrade));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("regenerate_all", "- Regenerates all zones defined by the configuration.", delegate (Terminal.ConsoleEventArgs args) {
        Operations.SetOperation(args.Context, Operation.Regenerate, GetZones(args.Context, Operation.Regenerate));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("pregenerate_all", "- Generates all missing zones defined by the configuration.", delegate (Terminal.ConsoleEventArgs args) {
        Operations.SetOperation(args.Context, Operation.Generate, GetZones(args.Context, Operation.Generate));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("regenerate_position", "[x] [y] [radius=0] - Regenerates zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing coordinate X");
          return;
        }
        if (args.Length < 3) {
          args.Context.AddString("Missing coordinate Z");
          return;
        }
        if (!float.TryParse(args[1], out var x)) {
          args.Context.AddString("Invalid format for X coordinate.");
          return;
        }
        if (!float.TryParse(args[2], out var z)) {
          args.Context.AddString("Invalid format for Z coordinate.");
          return;
        }
        var radius = 0f;
        if (args.Length > 3) {
          if (!float.TryParse(args[3], out radius)) {
            args.Context.AddString("Invalid format for radius.");
            return;
          }
        }
        var position = new Vector3(x, 0, z);
        var zone = ZoneSystem.instance.GetZone(position);
        if (!Settings.RegenerateLoadedAreas && ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is loaded. Move away and wait up to 10 seconds to unload it.");
          return;
        }
        Operations.SetOperation(args.Context, Operation.Regenerate, GetIncludedZones(args.Context, position, radius, Operation.Regenerate));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("regenerate_zones", "[x] [y] [adjacent=0] - Regenerates zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing zone x");
          return;
        }
        if (args.Length < 3) {
          args.Context.AddString("Missing zone y");
          return;
        }
        if (!int.TryParse(args[1], out var x)) {
          args.Context.AddString("Invalid format for X coordinate.");
          return;
        }
        if (!int.TryParse(args[2], out var z)) {
          args.Context.AddString("Invalid format for Z coordinate.");
          return;
        }
        var adjacent = 0;
        if (args.Length > 3) {
          if (!int.TryParse(args[3], out adjacent)) {
            args.Context.AddString("Invalid format for adjacency.");
            return;
          }
        }
        var zone = new Vector2i(x, z);
        if (!Settings.RegenerateLoadedAreas && ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is loaded. Move away and wait up to 10 seconds to unload it.");
          return;
        }
        Operations.SetOperation(args.Context, Operation.Regenerate, GetAdjacentZones(args.Context, zone, adjacent, Operation.Regenerate));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("pregenerate_position", "[x] [y] [radius=0] - Pregenerates zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing coordinate X");
          return;
        }
        if (args.Length < 3) {
          args.Context.AddString("Missing coordinate Z");
          return;
        }
        if (!float.TryParse(args[1], out var x)) {
          args.Context.AddString("Invalid format for X coordinate.");
          return;
        }
        if (!float.TryParse(args[2], out var z)) {
          args.Context.AddString("Invalid format for Z coordinate.");
          return;
        }
        var radius = 0f;
        if (args.Length > 3) {
          if (!float.TryParse(args[3], out radius)) {
            args.Context.AddString("Invalid format for radius.");
            return;
          }
        }
        var position = new Vector3(x, 0, z);
        var zone = ZoneSystem.instance.GetZone(position);
        if (ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is already generated.");
          return;
        }
        Operations.SetOperation(args.Context, Operation.Generate, GetIncludedZones(args.Context, position, radius, Operation.Generate));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("pregenerate_zones", "[x] [y] [adjacent=0] - Pregenerates zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing zone x");
          return;
        }
        if (args.Length < 3) {
          args.Context.AddString("Missing zone y");
          return;
        }
        if (!int.TryParse(args[1], out var x)) {
          args.Context.AddString("Invalid format for X coordinate.");
          return;
        }
        if (!int.TryParse(args[2], out var z)) {
          args.Context.AddString("Invalid format for Z coordinate.");
          return;
        }
        var adjacent = 0;
        if (args.Length > 3) {
          if (!int.TryParse(args[3], out adjacent)) {
            args.Context.AddString("Invalid format for adjacency.");
            return;
          }
        }
        var zone = new Vector2i(x, z);
        if (ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is already generated.");
          return;
        }
        Operations.SetOperation(args.Context, Operation.Generate, GetAdjacentZones(args.Context, zone, adjacent, Operation.Generate));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("query", "- Returns how many zones would get operated with current config", delegate (Terminal.ConsoleEventArgs args) {
        _ = GetZones(args.Context, Operation.Query);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "- Stops execution of current operation.", delegate (Terminal.ConsoleEventArgs args) {
        Operations.SetOperation(args.Context, Operation.None, new Vector2i[0]);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("redistribute", "- Redistributes unplaced locations with a modified genloc command. ", delegate (Terminal.ConsoleEventArgs args) {
        Operations.RedistributeLocations(true, false);
      }, onlyServer: true);
    }
  }
}