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
        "mistlands",
        "old_mistlands"
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
      if (type == "tarpits") {
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
        new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, args, Context);
      } else if (type == "mistlands") {
        if (args.Biomes.Count() > 0) {
          Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
          return;
        }
        args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Mistlands };
        Executor.AddOperation(new Destroy(Context, args));
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
      } else
        Print("Error: Invalid upgrade type");
    }
  }
}