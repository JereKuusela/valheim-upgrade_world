using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys and spawns given locations.</summary>
public class RegenerateLocations : LocationOperation
{
  public RegenerateLocations(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args)
  {
    Operation = "Reset locations";
    InitString = args.Print("Reset locations at");
    Verb = "reseted";
    Filterers = [.. Filterers, new LocationFilterer(ids, false)];
  }

  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location)
  {
    if (!location.m_placed) return false;
    location.m_placed = false;
    ZoneSystem.instance.m_locationInstances[zone] = location;
    if (location.m_location?.m_prefab == null)
    {
      Print("Location " + (location.m_location?.m_prefab.Name ?? "???") + " is missing at " + zone.ToString());
      return false;
    }

    var clearRadius = location.m_location.m_exteriorRadius;
    if (Args.ObjectReset.HasValue) clearRadius = Args.ObjectReset.Value;
    SpawnLocation(zone, location, clearRadius);
    AddPin(location.m_position);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefab.Name + " reseted at " + zone.ToString());
    return true;
  }
}
