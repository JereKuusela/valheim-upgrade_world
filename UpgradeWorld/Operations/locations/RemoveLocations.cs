using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys given locations.</summary>
public class RemoveLocations : LocationOperation {
  IEnumerable<string> Ids;
  public RemoveLocations(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context, args) {
    Operation = "Remove locations";
    InitString = args.Print($"Remove locations{Helper.IdString(ids)} from");
    Verb = "removed";
    Ids = ids;
    Filterers = Filterers.Append(new LocationFilterer(ids)).ToList();
  }

  ///<summary>Removes locations from ungenerated zones.</summary>
  protected override bool OnExecute() {
    var zs = ZoneSystem.instance;
    Args.TargetZones = TargetZones.Ungenerated;
    var filterers = FiltererFactory.Create(Args);
    filterers = filterers.Append(new LocationFilterer(Ids)).ToList();
    List<string> messages = new();
    var unplacedZones = filterers.Aggregate(Zones.GetWorldZones(), (zones, filterer) => filterer.FilterZones(zones, ref messages));
    foreach (var zone in unplacedZones) {
      if (!Args.Roll()) continue;
      PreOperated++;
      if (!zs.m_locationInstances.TryGetValue(zone, out var location)) continue;
      if (Settings.Verbose)
        Print("Location " + location.m_location.m_prefabName + " removed at " + zone.ToString());
      Operated++;
      zs.m_locationInstances.Remove(zone);
    }
    return base.OnExecute();
  }

  ///<summary>Removes locations from generated zones.</summary>
  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
    if (location.m_placed) Helper.ClearAreaForLocation(zone, location, true);
    ZoneSystem.instance.m_locationInstances.Remove(zone);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " removed at " + zone.ToString());
    return true;
  }
}
