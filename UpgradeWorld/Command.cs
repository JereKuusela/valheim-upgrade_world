using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
    private static bool ParseInt(string arg, out int number) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
    private static bool TryParseFloat(string arg, out float number) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    private static bool IsFloat(string arg) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
    private static float ParseFloat(string arg) => float.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture);
    private static IEnumerable<string> ParseArgs(IEnumerable<string> args, int skip) => string.Join(",", args.Skip(skip)).Split(',').Select(arg => arg.Trim()).Where(arg => arg != "");
    private static List<string> AvailableBiomes = new List<string>{
      "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains", "Swamp"
    };
    /// <summary>Converts a biome name to a biome.</summary>
    private static Heightmap.Biome GetBiome(string name) {
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
    private static IEnumerable<string> ParseFiltererArgs(Terminal.ConsoleEventArgs args, FiltererParameters parameters) {
      var parsed = ParseArgs(args.Args, 1);
      var other = parsed.Where(arg => !TryParseFloat(arg, out var _));
      var allNumbers = parsed.Where(arg => TryParseFloat(arg, out var _)).Select(ParseFloat);
      var ranges = other.Where(arg => arg.Split('-').Length == 2 && arg.Split('-').All(IsFloat));
      var range = ranges.FirstOrDefault();
      // Add back unused ranges.
      other.ToList().AddRange(ranges.Skip(1));
      if (range != null) {
        var split = range.Split('-').Select(ParseFloat);
        parameters.MinDistance = split.First();
        parameters.MaxDistance = split.Last();
      }
      var numbers = allNumbers.Take(range == null ? 3 : 2);
      // Add back unused numbers.
      other.ToList().AddRange(allNumbers.Skip(numbers.Count()).Select(arg => arg.ToString()));

      if (numbers.Count() == 1 || numbers.Count() == 3) {
        var distance = numbers.First();
        parameters.MinDistance = 0;
        parameters.MaxDistance = 0;
        if (distance > 0)
          parameters.MinDistance = distance;
        else
          parameters.MaxDistance = -distance;
        if (numbers.Count() == 3) {
          parameters.X = numbers.Skip(1).First();
          parameters.Y = numbers.Last();
        } else {
          parameters.X = Player.m_localPlayer.transform.position.x;
          parameters.Y = Player.m_localPlayer.transform.position.y;
        }
      }
      if (numbers.Count() == 2) {
        parameters.MinDistance = 0;
        parameters.MaxDistance = 0;
        parameters.X = numbers.First();
        parameters.Y = numbers.Last();
      }

      parameters.Biomes = other.Select(GetBiome).Where(biome => biome != Heightmap.Biome.BiomesMax && biome != Heightmap.Biome.None);
      other = other.Where(arg => GetBiome(arg) == Heightmap.Biome.None);
      other = ParseFlag(other, "ignorebase", out parameters.NoPlayerBase);
      other = ParseFlag(other, "zones", out parameters.MeasureWithZones);
      other = ParseFlag(other, "noedges", out parameters.IncludeEdges);
      parameters.TargetZones = TargetZones.Generated;
      return other;
    }

    private static bool ParseIncludedArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float distance) {
      x = 0;
      z = 0;
      distance = 0;
      if (args.Length < 2) {
        args.Context.AddString("Error: Missing coordinate X");
        return false;
      }
      if (args.Length < 3) {
        args.Context.AddString("Error: Missing coordinate Z");
        return false;
      }
      if (!TryParseFloat(args[1], out x)) {
        args.Context.AddString("Error: Invalid format for X coordinate.");
        return false;
      }
      if (!TryParseFloat(args[2], out z)) {
        args.Context.AddString("Error: Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!TryParseFloat(args[3], out distance)) {
          args.Context.AddString("Error: Invalid format for distance.");
          return false;
        }
      }
      return true;
    }
    private static IEnumerable<string> ParseFlag(IEnumerable<string> parameters, string flag, out bool value) {
      value = parameters.FirstOrDefault(arg => arg.ToLower() == flag) == null;
      return parameters.Where(arg => arg.ToLower() != flag);
    }
    private static void Mapping() {
      new Terminal.ConsoleCommand("reveal_position", "[x] [y] [distance=0] - Explores the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new ExploreMap(position, distance, true, args.Context);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("hide_position", "[x] [y] [distance=0] - Hides the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new ExploreMap(position, distance, false, args.Context);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("remove_pins", "[x] [y] [distance=0] - Removes pins from the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new RemovePins(position, distance, args.Context);
      }, onlyServer: true);

    }
    private static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0) {
      if (extra.Count() > handled) {
        args.Context.AddString("Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
        return false;
      }
      return true;
    }
    public static void Init() {
      new Terminal.ConsoleCommand("destroy", "[...args] - Destroys zones which allows the world generator to regenerate them.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = ParseFiltererArgs(args, parameters);
        if (CheckUnhandled(args, extra))
          Executor.AddOperation(new Destroy(args.Context, parameters));
      }, onlyServer: true, optionsFetcher: () => AvailableBiomes);

      new Terminal.ConsoleCommand("generate", "[...args] - Generates zones which allows pregenerating the world without having to move there physically.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = ParseFiltererArgs(args, parameters);
        if (CheckUnhandled(args, extra))
          Executor.AddOperation(new Generate(args.Context, parameters));
      }, onlyServer: true, optionsFetcher: () => AvailableBiomes);

      Mapping();

      new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = ParseFiltererArgs(args, parameters);
        var type = extra.FirstOrDefault();
        if (CheckUnhandled(args, extra, 1))
          new Upgrade(args.Context, type, parameters);
      }, onlyServer: true, optionsFetcher: Upgrade.GetTypes);

      new Terminal.ConsoleCommand("place_locations", "[...location_ids] [...args] - Places given location ids to already generated zones.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = ParseFiltererArgs(args, parameters);
        if (ids.Count() == 0) {
          args.Context.AddString("Error: Missing location ids.");
          return;
        }
        // TODO: Should validate location ids / provide parameter list.
        Executor.AddOperation(new DistributeLocations(ids, args.Context));
        Executor.AddOperation(new PlaceLocations(args.Context, parameters));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("reroll_chests", "[chest_name] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = ParseFiltererArgs(args, parameters);
        if (extra.Count() == 0) {
          args.Context.AddString("Error: Missing chest name.");
          return;
        }
        var chestName = extra.First();
        var ids = extra.Skip(1);
        new RerollChests(chestName, ids, args.Context);
      }, onlyServer: true, optionsFetcher: () => RerollChests.ChestsNames);
      new Terminal.ConsoleCommand("start", "- Starts execution of operations.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.DoExecute = true;
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.RemoveOperations();
      }, onlyServer: true);
      new Terminal.ConsoleCommand("count_biomes", "[frequency=100] [...args] - Counts amounts of biomes.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = ParseFiltererArgs(args, parameters);
        var frequency = 100f;
        if (extra.Count() > 0 && !TryParseFloat(extra.First(), out frequency)) {
          args.Context.AddString("Error: Frequency has wrong format.");
          return;
        }
        if (CheckUnhandled(args, extra, 1))
          new CountBiomes(args.Context, frequency, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("count_entities", "[showzero] [...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = ParseFiltererArgs(args, parameters);
        ids = ParseFlag(ids, "showzero", out var showZero);
        if (ids.Count() == 0)
          new CountAllEntities(args.Context, showZero, parameters);
        else
          new CountEntities(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("list_entities", "[...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = ParseFiltererArgs(args, parameters);
        new ListEntityPositions(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("remove_entities", "[...ids] [...args] - Removes entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = ParseFiltererArgs(args, parameters);
        new RemoveEntities(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new DistributeLocations(new string[0], args.Context));
      }, onlyServer: true);
    }
  }
}