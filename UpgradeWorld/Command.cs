using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
    private static bool ParseInt(string arg, out int number) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
    private static bool ParseFloat(string arg, out float number) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    private static IEnumerable<string> ParseArgs(IEnumerable<string> args, int skip) => string.Join(",", args.Skip(skip)).Split(',').Select(arg => arg.Trim()).Where(arg => arg != "");
    private static List<string> AvailableBiomes = new List<string>{
      "*", "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains", "Swamp"
    };
    /// <summary>Converts a biome name to a biome.</summary>
    private static Heightmap.Biome GetBiome(string name) {
      name = Helper.Normalize(name);
      var possibleBiomes = new List<Heightmap.Biome>();
      if (name == "*") possibleBiomes.Add(Heightmap.Biome.BiomesMax);
      if ("ashlands".StartsWith(name) || name == "al") possibleBiomes.Add(Heightmap.Biome.AshLands);
      if ("blackforest".StartsWith(name) || name == "bf") possibleBiomes.Add(Heightmap.Biome.BlackForest);
      if ("deepnorth".StartsWith(name) || name == "dn") possibleBiomes.Add(Heightmap.Biome.DeepNorth);
      if ("meadows".StartsWith(name)) possibleBiomes.Add(Heightmap.Biome.Meadows);
      if ("mistlands".StartsWith(name) || name == "ml") possibleBiomes.Add(Heightmap.Biome.Mistlands);
      if ("mountain".StartsWith(name)) possibleBiomes.Add(Heightmap.Biome.Mountain);
      if ("ocean".StartsWith(name)) possibleBiomes.Add(Heightmap.Biome.Ocean);
      if ("plains".StartsWith(name)) possibleBiomes.Add(Heightmap.Biome.Plains);
      if ("swamp".StartsWith(name)) possibleBiomes.Add(Heightmap.Biome.Swamp);
      if (possibleBiomes.Count == 1) return possibleBiomes.First();
      return Heightmap.Biome.None;
    }
    private static bool ParseArgsWithDistance(Terminal.ConsoleEventArgs args, out IEnumerable<string> ids, out float distance) {
      ids = new List<string>();
      distance = 0;
      if (args.Length < 2) {
        args.Context.AddString("Error: Missing ids.");
        return false;
      }
      var parsed = ParseArgs(args.Args, 1);
      ids = parsed.Where(arg => !ParseFloat(arg, out var _));
      var other = parsed.Where(arg => ParseFloat(arg, out var _));
      if (other.Count() == 1)
        distance = float.Parse(other.First());
      return true;
    }
    private static bool ParseBiomes(Terminal.ConsoleEventArgs args, out IEnumerable<Heightmap.Biome> biomes, out bool includeEdges) {
      biomes = new List<Heightmap.Biome>();
      includeEdges = true;
      if (args.Length < 2) {
        args.Context.AddString("Error: Missing biomes");
        biomes = null;
        return false;
      }
      var parsed = ParseArgs(args.Args, 1);
      // Max biome means no filtering.
      biomes = parsed.Where(arg => arg.ToLower() != "true" && arg.ToLower() != "false").Select(GetBiome).Where(biome => biome != Heightmap.Biome.BiomesMax);
      if (biomes.Contains(Heightmap.Biome.None)) {
        args.Context.AddString("Error: Invalid biomes");
        biomes = null;
        return false;
      }
      var other = parsed.Where(arg => arg.ToLower() == "true" || arg.ToLower() == "false");
      if (other.Count() == 1)
        includeEdges = other.First().ToLower() == "true";
      return true;
    }
    private static bool ParseZoneArgs(Terminal.ConsoleEventArgs args, out int x, out int z, out int adjacent) {
      x = 0;
      z = 0;
      adjacent = 0;
      if (args.Length < 2) {
        args.Context.AddString("Error: Missing zone x");
        return false;
      }
      if (args.Length < 3) {
        args.Context.AddString("Error: Missing zone y");
        return false;
      }
      if (!ParseInt(args[1], out x)) {
        args.Context.AddString("Error: Invalid format for X coordinate.");
        return false;
      }
      if (!ParseInt(args[2], out z)) {
        args.Context.AddString("Error: Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!ParseInt(args[3], out adjacent)) {
          args.Context.AddString("Error: Invalid format for adjacency.");
          return false;
        }
      }
      return true;
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
      if (!ParseFloat(args[1], out x)) {
        args.Context.AddString("Error: Invalid format for X coordinate.");
        return false;
      }
      if (!ParseFloat(args[2], out z)) {
        args.Context.AddString("Error: Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!ParseFloat(args[3], out distance)) {
          args.Context.AddString("Error: Invalid format for distance.");
          return false;
        }
      }
      return true;
    }
    private static void Destroying() {
      new Terminal.ConsoleCommand("destroy_biomes", "[biome1, biome2, ...] [includeEdges=true] - Destroys all zones in given biomes.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseBiomes(args, out var biomes, out var includeEdges)) return;
        Executor.AddOperation(new DestroyBiomes(biomes, includeEdges, args.Context));
      }, onlyServer: true, optionsFetcher: () => AvailableBiomes);
      new Terminal.ConsoleCommand("destroy_position", "[x] [y] [distance=0] - Destroys zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        Executor.AddOperation(new DestroyIncluded(position, distance, args.Context));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("destroy_zones", "[x] [y] [adjacent=0] - Destroys zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseZoneArgs(args, out int x, out int z, out int adjacent)) return;
        var zone = new Vector2i(x, z);
        Executor.AddOperation(new DestroyAdjacent(zone, adjacent, args.Context));
      }, onlyServer: true);
    }
    private static void Generating() {
      new Terminal.ConsoleCommand("generate_biomes", "[biome1, biome2, ...] [includeEdges=true] - Generates all zones in given biomes.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseBiomes(args, out var biomes, out var includeEdges)) return;
        Executor.AddOperation(new GenerateBiomes(biomes, includeEdges, args.Context));
      }, onlyServer: true, optionsFetcher: () => AvailableBiomes);

      new Terminal.ConsoleCommand("generate_position", "[x] [y] [distance=0] - Generates zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        Executor.AddOperation(new GenerateIncluded(position, distance, args.Context));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("generate_zones", "[x] [y] [adjacent=0] - Generates zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseZoneArgs(args, out int x, out int z, out int adjacent)) return;
        var zone = new Vector2i(x, z);
        Executor.AddOperation(new GenerateAdjacent(zone, adjacent, args.Context));
      }, onlyServer: true);
    }
    public static void Init() {
      Destroying();
      Generating();
      new Terminal.ConsoleCommand("upgrade", "[operation] - Performs a predefined upgrade operation.", delegate (Terminal.ConsoleEventArgs args) {
        var valid = Upgrade.GetTypes();
        if (!valid.Contains(args[1])) {
          args.Context.AddString("Error: Invalid upgrade operation.");
          return;
        }
        Executor.AddOperation(new Upgrade(args[1], args.Context));
      }, onlyServer: true, optionsFetcher: Upgrade.GetTypes);
      new Terminal.ConsoleCommand("place_locations", "[location1, location2, ...] - Places given location ids to already generated zones.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Error: Missing location ids.");
          return;
        }
        var ids = ParseArgs(args.Args, 1);
        Executor.AddOperation(new DistributeLocations(ids, args.Context));
        Executor.AddOperation(new PlaceLocations(args.Context));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("reroll_chests", "[chest name] [item1, item2, ...] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Error: Missing chest name.");
          return;
        }
        var ids = ParseArgs(args.Args, 2);
        Executor.AddOperation(new RerollChests(args[1], ids, args.Context));
      }, onlyServer: true, optionsFetcher: () => RerollChests.ChestsNames);
      new Terminal.ConsoleCommand("query", "- Returns how many zones would get operated with current config", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new Query(args.Context));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "- Stops execution of current operation.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.RemoveOperation();
      }, onlyServer: true);
      new Terminal.ConsoleCommand("count", "[id1, id2, id3] [distance=0] - Counts amounts of given entity ids within a distance (0 for infinite).", delegate (Terminal.ConsoleEventArgs args) {
        ParseArgsWithDistance(args, out var ids, out var distance);
        Executor.AddOperation(new Count(ids, distance, args.Context));
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("count_all", " [distance] - Counts all entities within a distance (0 for infinite).", delegate (Terminal.ConsoleEventArgs args) {
        var distance = 0f;
        if (args.Length < 2) {
          args.Context.AddString("Error: Missing distance.");
          return;
        }
        if (!ParseFloat(args[1], out distance)) {
          args.Context.AddString("Error: Invalid format distance.");
          return;
        }
        Executor.AddOperation(new Count(new string[0], distance, args.Context));
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("remove", "[id1, id2, id3] [distance=0] - Removes given entity ids within a distance (0 for infinite).", delegate (Terminal.ConsoleEventArgs args) {
        ParseArgsWithDistance(args, out var ids, out var distance);
        Executor.AddOperation(new Remove(ids, distance, args.Context));
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command. ", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new DistributeLocations(new string[0], args.Context));
      }, onlyServer: true);
    }
  }
}