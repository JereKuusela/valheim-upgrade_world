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
        "mistlands"
      };
    }
    public Upgrade(Terminal context, string type, IEnumerable<string> extra, FiltererParameters args) : base(context) {
      AddOperations(type, extra, args);
    }

    private void AddOperations(string type, IEnumerable<string> extra, FiltererParameters args) {
      if (type == null) {
        Print("Error: Missing upgrade type");
      }
      type = type.ToLower();
      if (type == "tarpits") {
        extra = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit1" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit2" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit3" }, Context));
        Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      } else if (type == "onions") {
        if (extra.Count() > 0) {
          Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
          return;
        }
        new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, args, Context);
      } else if (type == "mistlands") {
        if (args.Biomes != null) {
          Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
          return;
        }
        args.Biomes = new List<Heightmap.Biome> { Heightmap.Biome.Mistlands };
        Executor.AddOperation(new Destroy(Context, args));
      }
      Print("Error: Invalid upgrade type");
    }
  }
}