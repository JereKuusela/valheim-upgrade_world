using System.Collections.Generic;

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
    public Upgrade(Terminal context, string type, FiltererParameters args) : base(context) {
      AddOperations(type, args);
    }

    private void AddOperations(string type, FiltererParameters args) {
      if (type == null) {
        Print("Error: Missing upgrade type");
      }
      type = type.ToLower();
      if (type == "tarpits") {
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit1" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit2" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit3" }, Context));
        Executor.AddOperation(new PlaceLocations(Context, args));
      } else if (type == "onions") {
        new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, Context);
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