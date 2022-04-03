using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys given locations.</summary>
public class RemoveLocations : LocationOperation {
  IEnumerable<string> Ids;
  public RemoveLocations(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context, args) {
    Operation = "Remove locations";
    InitString = args.Print("Remove locations at");
    Verb = "removed";
    Ids = ids;
    Filterers = Filterers.Append(new LocationFilterer(ids));
  }

  ///<summary>Removes locations from ungenerated zones.</summary>
  protected override bool OnExecute() {
    var zs = ZoneSystem.instance;
    var locationObjects = Ids.Select(id => id.GetStableHashCode()).ToHashSet();
    var unplacedZones = zs.m_locationInstances.Where(kvp => !kvp.Value.m_placed).Where(kvp => locationObjects.Contains(kvp.Value.m_location.m_prefabName.GetStableHashCode())).Select(kvp => kvp.Key).ToHashSet();
    foreach (var zone in unplacedZones) {
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
