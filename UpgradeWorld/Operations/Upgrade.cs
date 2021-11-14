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
    public Upgrade(string type, Terminal context) : base(context) {
      AddOperations(type);
    }

    private void AddOperations(string type) {
      if (type == "tarpits") {
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit1" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit2" }, Context));
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit3" }, Context));
        Executor.AddOperation(new PlaceLocations(Context));
      }
      if (type == "onions") {
        new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, Context);
      }
      if (type == "mistlands") {
        Executor.AddOperation(new DestroyBiomes(new List<Heightmap.Biome> { Heightmap.Biome.Mistlands }, true, Context));
      }
    }
  }
}