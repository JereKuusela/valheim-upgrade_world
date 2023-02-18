using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys and spawns given locations.</summary>
public class RegenerateLocations : LocationOperation
{
  public RegenerateLocations(Terminal context, string[] ids, FiltererParameters args) : base(context, args)
  {
    Operation = "Reset locations";
    InitString = args.Print("Reset locations at");
    Verb = "reseted";
    if (ids.Length > 0)
      Filterers = Filterers.Append(new LocationFilterer(ids)).ToList();
  }

  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location)
  {
    if (!location.m_placed) return false;
    location.m_placed = false;
    ZoneSystem.instance.m_locationInstances[zone] = location;
    if (location.m_location?.m_prefab == null)
    {
      Print("Location " + (location.m_location?.m_prefabName ?? "???") + " is missing at " + zone.ToString());
      return false;
    }
    SpawnLocation(zone, location, true, true);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " reseted at " + zone.ToString());
    return true;
  }
}
