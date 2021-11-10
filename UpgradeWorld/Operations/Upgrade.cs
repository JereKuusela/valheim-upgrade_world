using System.Collections.Generic;

namespace UpgradeWorld {
  /// <summary>Counts the amount of a given entity id within a given radius.</summary>
  public class Upgrade : BaseOperation {
    public static List<string> GetTypes() {
      return new List<string>()
      {
        "tarpits",
        "onions",
        "mistlands"
      };
    }
    string Type;
    public Upgrade(string type, Terminal context) : base(context) {
      Type = type;
    }

    protected override bool OnExecute() {
      if (Type == "tarpits") {
        var locations = new string[] { "TarPit1", "TarPit2", "TarPit3" };
        Executor.AddOperation(new DistributeLocations(locations, Context));
        Executor.AddOperation(new PlaceLocations(locations, Context));
      }
      if (Type == "onions") {
        Executor.AddOperation(new RerollChests("TreasureChest_mountains", new string[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, Context));
      }
      if (Type == "mistlands") {
        Executor.AddOperation(new DestroyBiomes(new List<Heightmap.Biome> { Heightmap.Biome.Mistlands }, true, Context));
      }
      return true;
    }
  }
}