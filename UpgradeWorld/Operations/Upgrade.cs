using System.Collections.Generic;

namespace UpgradeWorld {
  /// <summary>Counts the amount of a given entity id within a given radius.</summary>
  public class Upgrade : BaseOperation {
    public static List<string> GetTypes() {
      return new List<string>()
      {
        "tarpits"
      };
    }
    string Type;
    public Upgrade(string type, Terminal context) : base(context) {
      Type = type;
    }

    public override bool Execute() {
      if (Type == "tarpits") {
        Executor.AddOperation(new DistributeLocations(new string[] { "TarPit1", "TarPit2", "TarPit3" }, true, Context));
        Executor.AddOperation(new PlaceLocations(Context));
      }
      return base.Execute();
    }
  }
}