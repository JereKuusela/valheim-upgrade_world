using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {

  /// <summary>Distributes given location ids to already generated zones.</summary>
  public class DistributeLocations : BaseOperation {
    public static IEnumerable<string> Ids = new string[0];
    public static bool PlaceToAlreadyGenerated => Ids.Count() > 0;
    public DistributeLocations(IEnumerable<string> ids, Terminal context) : base(context) {
      Ids = ids;
    }
    protected override bool OnExecute() {
      if (Attempts == 1) {
        Print("Redistributing locations (" + Helper.JoinRows(Ids) + ") to old areas. This may take a while...");
        return false;
      } else {
        ZoneSystem.instance.GenerateLocations();
        Ids = new string[0];
        return true;
      }
    }
    protected override void OnEnd() {
      Print("Locations (" + Helper.JoinRows(Ids) + ") distributed.");
    }
  }
}