using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Distributes given location ids to already generated zones.</summary>
public class DistributeLocations : ExecutedOperation {
  public static IEnumerable<string> DistributedIds = new string[0];
  private IEnumerable<string> Ids = new string[0];
  public float Chance = 1f;
  public int Added = 0;
  public static bool PlaceToAlreadyGenerated = false;
  public DistributeLocations(IEnumerable<string> ids, bool autoStart, float chance, Terminal context) : base(context, autoStart) {
    Ids = ids;
    Chance = chance;
  }
  public static bool IsIncluded(ZoneSystem.ZoneLocation location) => DistributedIds.Count() == 0 || DistributedIds.Contains(location.m_prefabName.ToLower());
  protected override bool OnExecute() {
    if (Attempts == 1) {
      Print($"Redistributing locations{Helper.IdString(Ids)}. This may take a while...");
      return false;
    } else {
      PlaceToAlreadyGenerated = true;
      DistributedIds = Ids.Select(id => id.ToLower());
      var before = ZoneSystem.instance.m_locationInstances.Count;
      ZoneSystem.instance.GenerateLocations();
      if (Chance < 1f) {
        ZoneSystem.instance.m_locationInstances = ZoneSystem.instance.m_locationInstances
          .Where(kvp => kvp.Value.m_placed || !IsIncluded(kvp.Value.m_location) || FiltererParameters.random.NextDouble() < Chance)
          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      }
      Added = ZoneSystem.instance.m_locationInstances.Count - before;
      PlaceToAlreadyGenerated = false;
      DistributedIds = new string[0];
      return true;
    }
  }
  protected override void OnEnd() {
    if (Settings.Verbose) {
      if (Added >= 0)
        Print($"{Added} locations{GetLocationString()}added.");
      else
        Print($"{Math.Abs(Added)} locations{GetLocationString()}removed.");
    } else
      Print($"Locations{GetLocationString()}added.");
  }

  private string GetLocationString() {
    if (Ids.Count() == 0) return " ";
    return " " + Helper.JoinRows(Ids) + " ";
  }

  protected override string OnInit() {
    return "Redistribute locations" + GetLocationString() + "to all areas.";
  }
}
