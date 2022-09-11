using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Predefined upgrade operations</summary>
public class Upgrade : BaseOperation {
  public static List<string> Types = new()
  {
    "tarpits",
    "onions",
    "mistlands",
    "mountain_caves",
    "EVA"
  };

  public Upgrade(Terminal context, string type, Dictionary<string, string> extra, FiltererParameters args) : base(context) {
    AddOperations(type, extra, args);
    Types.Sort();
  }

  private void AddOperations(string type, Dictionary<string, string> extra, FiltererParameters args) {
    if (type == null || type == "") {
      Print("Error: Missing upgrade type");
      return;
    }
    type = type.ToLower();
    if (type == "mountain_caves") {
      var noClearing = Parse.Flag(extra, "noclearing");
      Executor.AddOperation(new DistributeLocations(new[] { "MountainCave02" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else if (type == "tarpits") {
      var noClearing = Parse.Flag(extra, "noclearing");
      Executor.AddOperation(new DistributeLocations(new[] { "TarPit1", "TarPit2", "TarPit3" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else if (type == "onions") {
      if (extra.Count() > 0) {
        Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
        return;
      }
      new ResetChests(new[] { "TreasureChest_mountains" }, new[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, args.Start, new(args), Context);
    } else if (type == "mistlands") {
      if (args.Biomes.Count() > 0) {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Mistlands };
      Executor.AddOperation(new ResetZones(Context, args));
      Executor.AddOperation(new DistributeLocations(new string[0], args.Start, args.Chance, Context));
    } else if (type == "eva") {
      if (args.Biomes.Count() > 0) {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      var noClearing = Parse.Flag(extra, "noclearing");
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Plains, Heightmap.Biome.DeepNorth, Heightmap.Biome.Mistlands, Heightmap.Biome.AshLands };
      var safeZones = args.SafeZones;
      args.SafeZones = 0;
      Executor.AddOperation(new RemoveLocations(Context, new[] { "SvartalfrQueenAltar" }, args));
      args.SafeZones = safeZones;
      Executor.AddOperation(new DistributeLocations(new[] { "BlazingDamnedOneAltar", "JotunnAltar", "SvartalfrQueenAltar_New", "Vegvisir_BlazingDamnedOne", "Vegvisir_Jotunn", "Vegvisir_SvartalfrQueen" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      Executor.AddOperation(new RemoveVegetation(Context, new() { "BurningTree", "FrometalVein_frac", "HeavymetalVein" }, args));
      Executor.AddOperation(new AddVegetation(Context, new() { "BurningTree", "FrometalVein_frac", "HeavymetalVein" }, args));
    } else
      Print("Error: Invalid upgrade type");
  }
}
