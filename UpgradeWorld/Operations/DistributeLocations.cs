using System.Linq;

namespace UpgradeWorld {

  /// <summary>Distributes given location ids to already generated zones.</summary>
  public class DistributeLocations : BaseOperation {
    public static string[] Ids;
    public static bool PlaceToAlreadyGenerated => Ids != null && Ids.Length > 0;
    public DistributeLocations(string[] ids, Terminal context) : base(context) {
      Ids = ids.Select(Helper.Normalize).ToArray();
    }
    protected override bool OnExecute() {
      if (Attempts == 1) {
        Print("Redistributing locations to old areas. This may take a while...");
        return false;
      } else {
        ZoneSystem.instance.GenerateLocations();
        Ids = new string[0];
        return true;
      }
    }
    protected override void OnEnd() {
      Print("Locations distributed.");
    }
  }
}