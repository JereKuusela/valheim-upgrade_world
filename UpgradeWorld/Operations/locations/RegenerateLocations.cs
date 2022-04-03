using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys and places given locations.</summary>
public class RegenerateLocations : LocationOperation {
  public RegenerateLocations(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context, args) {
    Operation = "Regenerate locations";
    InitString = args.Print("Regenerate locations at");
    Verb = "regenerated";
    Filterers = Filterers.Append(new LocationFilterer(ids));
  }

  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
    if (!location.m_placed) return false;
    location.m_placed = false;
    ZoneSystem.instance.m_locationInstances[zone] = location;
    PlaceLocation(zone, location, true, true);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " regenerated at " + zone.ToString());
    return true;
  }
}
