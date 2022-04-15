using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Distributes given location ids to already generated zones.</summary>
public class DistributeLocations : ExecutedOperation {
  public static IEnumerable<string> DistributedIds = new string[0];
  private IEnumerable<string> Ids = new string[0];
  public static bool PlaceToAlreadyGenerated => DistributedIds.Count() > 0;
  public DistributeLocations(IEnumerable<string> ids, bool autoStart, Terminal context) : base(context, autoStart) {
    Ids = ids;
  }
  protected override bool OnExecute() {
    if (Attempts == 1) {
      Print($"Redistributing locations{Helper.IdString(Ids)}. This may take a while...");
      return false;
    } else {
      DistributedIds = Ids.Select(id => id.ToLower());
      ZoneSystem.instance.GenerateLocations();
      DistributedIds = new string[0];
      return true;
    }
  }
  protected override void OnEnd() {
    Print("Locations" + GetLocationString() + "distributed.");
  }

  private string GetLocationString() {
    if (Ids.Count() == 0) return " ";
    return " " + Helper.JoinRows(Ids) + " ";
  }

  protected override string OnInit() {
    return "Redistribute locations" + GetLocationString() + "to all areas.";
  }
}
