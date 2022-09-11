using System;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Distributes given location ids to already generated zones.</summary>
public class DistributeLocations : ExecutedOperation {
  public static string[] DistributedIds = new string[0];
  private string[] Ids = new string[0];
  public float Chance = 1f;
  public int Added = 0;
  private int Total = 0;
  public static bool PlaceToAlreadyGenerated = false;
  public DistributeLocations(string[] ids, bool autoStart, float chance, Terminal context) : base(context, autoStart) {
    Ids = ids;
    if (Ids.Length == 0)
      Ids = ZoneSystem.instance.m_locations.Select(loc => loc.m_prefabName).ToArray();
    Chance = chance;
  }
  public static bool IsIncluded(ZoneSystem.ZoneLocation location) => DistributedIds.Count() == 0 || DistributedIds.Contains(location.m_prefabName);
  protected override bool OnExecute() {
    // Note: Printing is done one step before the execution, otherwise it would get printed afterwards.
    if (Attempts == 0) {
      if (Settings.Verbose)
        Print($"Redistributing locations {Ids[Attempts]}. This may take a while...");
      else
        Print($"Redistributing locations. This may take a while...");
      return false;
    }
    if (Attempts <= Ids.Length) {
      PlaceToAlreadyGenerated = true;
      DistributedIds = new string[] { Ids[Attempts - 1] };
      if (Settings.Verbose && Attempts < Ids.Length)
        Print($"Redistributing locations {Ids[Attempts]}. This may take a while...");
      var zs = ZoneSystem.instance;
      var before = zs.m_locationInstances.Count;
      zs.GenerateLocations();
      if (Chance < 1f) {
        zs.m_locationInstances = zs.m_locationInstances
          .Where(kvp => kvp.Value.m_placed || !IsIncluded(kvp.Value.m_location) || FiltererParameters.random.NextDouble() < Chance)
          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      }
      Total += zs.m_locationInstances.Where(kvp => IsIncluded(kvp.Value.m_location)).Count();
      Added += zs.m_locationInstances.Count - before;
      PlaceToAlreadyGenerated = false;
      DistributedIds = new string[0];
      return false;
    }
    return true;
  }
  protected override void OnEnd() {
    if (Settings.Verbose) {
      if (Added >= 0)
        Print($"{Added} locations{Helper.LocationIdString(Ids)} added (total amount: {Total}).");
      else
        Print($"{Math.Abs(Added)} locations{Helper.LocationIdString(Ids)} removed (total amount: {Total}).");
    } else
      Print($"Locations{Helper.LocationIdString(Ids)} added.");
  }


  protected override string OnInit() {
    return $"Redistribute locations{Helper.LocationIdString(Ids)} to all areas.";
  }
}
