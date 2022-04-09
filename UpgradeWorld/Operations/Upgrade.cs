using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Predefined upgrade operations</summary>
public class Upgrade : BaseOperation {
  public static List<string> Types = new()
  {
    "tarpits",
    "onions",
    "new_mistlands",
    "old_mistlands",
    "mountain_caves",
    "EVA_1.3+1.4",
    "EVA_1.3+1.4_locations_only",
    "EVA_1.4"
  };

  public Upgrade(Terminal context, string type, IEnumerable<string> extra, FiltererParameters args) : base(context) {
    AddOperations(type, extra, args);
    Types.Sort();
  }

  private void AddOperations(string type, IEnumerable<string> extra, FiltererParameters args) {
    if (type == null || type == "") {
      Print("Error: Missing upgrade type");
      return;
    }
    type = type.ToLower();
    if (type == "mountain_caves") {
      extra = Parse.Flag(extra, "noclearing", out var noClearing);
      Executor.AddOperation(new DistributeLocations(new[] { "MountainCave02" }, false, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else if (type == "tarpits") {
      extra = Parse.Flag(extra, "noclearing", out var noClearing);
      Executor.AddOperation(new DistributeLocations(new[] { "TarPit1" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "TarPit2" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "TarPit3" }, false, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else if (type == "onions") {
      if (extra.Count() > 0) {
        Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
        return;
      }
      new RerollChests("TreasureChest_mountains", new[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, false, args, Context);
    } else if (type == "new_mistlands") {
      if (args.Biomes.Count() > 0) {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Mistlands };
      Executor.AddOperation(new Destroy(Context, args));
      Executor.AddOperation(new DistributeLocations(new string[] { }, false, Context));
    } else if (type == "old_mistlands") {
      if (args.Biomes.Count() > 0) {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Mistlands };
      new SetVegetation(Context, true, new[] { "HugeRoot1", "SwampTree2_darkland", "Pinetree_01", "FirTree_small_dead", "vertical_web", "horizontal_web", "tunnel_web", "stubbe", "Skull1", "Skull2", "Rock_3", "Rock_4" });
      Executor.AddOperation(new Destroy(Context, args));
      args.TargetZones = TargetZones.All;
      Executor.AddOperation(new Generate(Context, args));
      Executor.AddCleanUp(() => new ResetVegetation(Context));
    } else if (type == "EVA_1.3+1.4") {
      if (args.Biomes.Count() > 0) {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Plains, Heightmap.Biome.DeepNorth, Heightmap.Biome.Mistlands, Heightmap.Biome.AshLands };
      Executor.AddOperation(new Destroy(Context, args));
      Executor.AddOperation(new DistributeLocations(new[] { "BlazingDamnedOneAltar" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "JotunnAltar" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "SvartalfrQueenAltar_New" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_BlazingDamnedOne" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_Jotunn" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_SvartalfrQueen" }, false, Context));
    } else if (type == "EVA_1.3+1.4_locations_only") {
      extra = Parse.Flag(extra, "noclearing", out var noClearing);
      Executor.AddOperation(new DistributeLocations(new[] { "BlazingDamnedOneAltar" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "JotunnAltar" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "SvartalfrQueenAltar_New" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_BlazingDamnedOne" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_Jotunn" }, false, Context));
      Executor.AddOperation(new DistributeLocations(new[] { "Vegvisir_SvartalfrQueen" }, false, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else if (type == "EVA_1.4") {
      extra = Parse.Flag(extra, "noclearing", out var noClearing);
      Executor.AddOperation(new RemoveLocations(Context, new[] { "SvartalfrQueenAltar" }, args));
      Executor.AddOperation(new DistributeLocations(new[] { "SvartalfrQueenAltar_New" }, false, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    } else
      Print("Error: Invalid upgrade type");
  }
}
