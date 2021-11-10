using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
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
    private static bool ParseBiomes(Terminal.ConsoleEventArgs args, out IEnumerable<Heightmap.Biome> biomes, out bool includeEdges) {
      biomes = new List<Heightmap.Biome>();
      includeEdges = true;
      if (args.Length < 2) {
        args.Context.AddString("Missing biomes");
        return false;
      }
      // Max biome means no filtering.
      biomes = args[1].Split(',').Select(GetBiome).Where(biome => biome != Heightmap.Biome.BiomesMax);
      if (biomes.Contains(Heightmap.Biome.None)) {
        args.Context.AddString("Invalid biomes");
        return false;
      }
      if (args.Length > 3)
        includeEdges = args[3].ToLower() == "true" || args[3].ToLower() == "1";
      return true;
    }
    private static bool ParseZoneArgs(Terminal.ConsoleEventArgs args, out int x, out int z, out int adjacent) {
      x = 0;
      z = 0;
      adjacent = 0;
      if (args.Length < 2) {
        args.Context.AddString("Missing zone x");
        return false;
      }
      if (args.Length < 3) {
        args.Context.AddString("Missing zone y");
        return false;
      }
      if (!int.TryParse(args[1], out x)) {
        args.Context.AddString("Invalid format for X coordinate.");
        return false;
      }
      if (!int.TryParse(args[2], out z)) {
        args.Context.AddString("Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!int.TryParse(args[3], out adjacent)) {
          args.Context.AddString("Invalid format for adjacency.");
          return false;
        }
      }
      return true;
    }
    private static bool ParseIncludedArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float radius) {
      x = 0;
      z = 0;
      radius = 0;
      if (args.Length < 2) {
        args.Context.AddString("Missing coordinate X");
        return false;
      }
      if (args.Length < 3) {
        args.Context.AddString("Missing coordinate Z");
        return false;
      }
      if (!float.TryParse(args[1], out x)) {
        args.Context.AddString("Invalid format for X coordinate.");
        return false;
      }
      if (!float.TryParse(args[2], out z)) {
        args.Context.AddString("Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!float.TryParse(args[3], out radius)) {
          args.Context.AddString("Invalid format for radius.");
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
      new Terminal.ConsoleCommand("destroy_position", "[x] [y] [radius=0] - Destroys zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var radius)) return;
        var position = new Vector3(x, 0, z);
        var zone = ZoneSystem.instance.GetZone(position);
        if (!Settings.DestroyLoadedAreas && ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is loaded. Move away and wait up to 10 seconds to unload it.");
          return;
        }
        Executor.AddOperation(new DestroyIncluded(position, radius, args.Context));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("destroy_zones", "[x] [y] [adjacent=0] - Destroys zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseZoneArgs(args, out int x, out int z, out int adjacent)) return;
        var zone = new Vector2i(x, z);
        if (!Settings.DestroyLoadedAreas && ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is loaded. Move away and wait up to 10 seconds to unload it.");
          return;
        }
        Executor.AddOperation(new DestroyAdjacent(zone, adjacent, args.Context));
      }, onlyServer: true);
    }
    private static void Generating() {
      new Terminal.ConsoleCommand("generate_biomes", "[biome1, biome2, ...] [includeEdges=true] - Generates all zones in given biomes.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseBiomes(args, out var biomes, out var includeEdges)) return;
        Executor.AddOperation(new GenerateBiomes(biomes, includeEdges, args.Context));
      }, onlyServer: true, optionsFetcher: () => AvailableBiomes);

      new Terminal.ConsoleCommand("generate_position", "[x] [y] [radius=0] - Generates zones at a given position.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseIncludedArgs(args, out var x, out var z, out var radius)) return;
        var position = new Vector3(x, 0, z);
        var zone = ZoneSystem.instance.GetZone(position);
        if (ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is already generated.");
          return;
        }
        Executor.AddOperation(new GenerateIncluded(position, radius, args.Context));
      }, onlyServer: true);

      new Terminal.ConsoleCommand("Generate_zones", "[x] [y] [adjacent=0] - Generates zones at a given zone coordinates.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ParseZoneArgs(args, out int x, out int z, out int adjacent)) return;
        var zone = new Vector2i(x, z);
        if (ZoneSystem.instance.IsZoneLoaded(zone)) {
          args.Context.AddString("Target zone is already generated.");
          return;
        }
        Executor.AddOperation(new GenerateAdjacent(zone, adjacent, args.Context));
      }, onlyServer: true);
    }
    public static void Init() {
      Destroying();
      Generating();
      new Terminal.ConsoleCommand("upgrade", "[type] - Performs a predefined upgrade operation.", delegate (Terminal.ConsoleEventArgs args) {
        var valid = Upgrade.GetTypes();
        if (!valid.Contains(args[1])) {
          args.Context.AddString("Invalid upgrade type.");
          return;
        }
        Executor.AddOperation(new Upgrade(args[1], args.Context));
      }, onlyServer: true, optionsFetcher: Upgrade.GetTypes);
      new Terminal.ConsoleCommand("place_locations", "[location1, location2, ...] - Places given location ids to already generated zones.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing location ids.");
          return;
        }
        var ids = args.Args[1].Split(',').Select(item => item.Trim());
        Executor.AddOperation(new DistributeLocations(ids, args.Context));
        Executor.AddOperation(new PlaceLocations(ids, args.Context));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("reroll_chests", "[chest name] [item1, item2, ...] - Rerolls items at given chests, if they only have given items (all chests if no items).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString("Missing chest name.");
          return;
        }
        var items = args.Length > 2 ? args[2].Split(',') : new string[0];
        Executor.AddOperation(new RerollChests(args[1], items, args.Context));
      }, onlyServer: true, optionsFetcher: () => RerollChests.ChestsNames);
      new Terminal.ConsoleCommand("query", "- Returns how many zones would get operated with current config", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new Query(args.Context));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "- Stops execution of current operation.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.RemoveOperation();
      }, onlyServer: true);
      new Terminal.ConsoleCommand("count", " [id1, id2, id3] [radius=0] - Counts amounts of given entity ids within a radius (0 for infinite).", delegate (Terminal.ConsoleEventArgs args) {
        var radius = 0f;
        if (args.Length < 2) {
          args.Context.AddString("Missing entity ids.");
          return;
        }
        if (args.Length > 2) {
          if (!float.TryParse(args[2], out radius)) {
            args.Context.AddString("Invalid format for radius.");
            return;
          }
        }
        Executor.AddOperation(new Count(args[1].Split(',').Select(id => id.Trim()), radius, args.Context));
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command. ", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new DistributeLocations(new string[0], args.Context));
      }, onlyServer: true);
    }
  }
}