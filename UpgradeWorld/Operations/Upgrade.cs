using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Predefined upgrade operations</summary>
  public class Upgrade : BaseOperation {
    public static List<string> GetTypes() {
      return new List<string>()
      {
        "tarpits",
        "onions",
        "new_mistlands",
        "old_mistlands",
      };
    }
    public Upgrade(Terminal context, string type, IEnumerable<string> extra, FiltererParameters args) : base(context) {
      AddOperations(type, extra, args);
    }

    private void AddOperations(string type, IEnumerable<string> extra, FiltererParameters args) {
      if (type == null || type == "") {
        Print("Error: Missing upgrade type");
        return;
      }
      type = type.ToLower();
      if (type == "mountain_caves") {
        extra = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        Executor.AddOperation(new DistributeLocations(new string[] { "MountainCave01" }, false, Context));
        Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      } else if (type == "tarpits") {
        extra = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit1" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit2" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit3" }, false, Context));
        Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      } else if (type == "onions") {
        if (extra.Count() > 0) {
          Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
          return;
        }
        new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, false, args, Context);
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
        new SetVegetation(Context, true, new string[] { "HugeRoot1", "SwampTree2_darkland", "Pinetree_01", "FirTree_small_dead", "vertical_web", "horizontal_web", "tunnel_web", "stubbe", "Skull1", "Skull2", "Rock_3", "Rock_4" });
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
        Executor.AddOperation(new DistributeLocations(new string[] { "BlazingDamnedOneAltar" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "JotunnAltar" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "SvartalfrQueenAltar_New" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_BlazingDamnedOne" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_Jotunn" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_SvartalfrQueen" }, false, Context));
      } else if (type == "EVA_1.3+1.4_locations_only") {
        extra = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        Executor.AddOperation(new DistributeLocations(new string[] { "BlazingDamnedOneAltar" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "JotunnAltar" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "SvartalfrQueenAltar_New" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_BlazingDamnedOne" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_Jotunn" }, false, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "Vegvisir_SvartalfrQueen" }, false, Context));
        Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      } else if (type == "EVA_1.4") {
        extra = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        Executor.AddOperation(new DistributeLocations(new string[] { "SvartalfrQueenAltar_New" }, false, Context));
        Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      } else
        Print("Error: Invalid upgrade type");
    }
  }
}