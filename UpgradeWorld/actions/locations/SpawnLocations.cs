using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld;
///<summary>Spawns locations in explored areas.</summary>
public class SpawnLocations : LocationOperation
{
  private readonly HashSet<string> Ids;
  public SpawnLocations(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args)
  {
    Operation = "Spawn missing locations";
    InitString = args.Print("Spawn missing locations to");
    Verb = "spawned to already generated areas";
    args.Chance = 1f;
    Ids = ids;
  }
  protected override void OnStart()
  {
    // This must be done right before execution to get the latest location data.
    // Doing it on the contructor would result in wrong zones if location distribute was executed before.
    var filterer = new LocationFilterer(Ids, true);
    List<string> messages = [];
    ZonesToUpgrade = filterer.FilterZones(ZonesToUpgrade, ref messages);
  }
  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location)
  {
    if (location.m_placed) return false;
    if (location.m_location?.m_prefab == null)
    {
      Print("Location " + (location.m_location?.m_prefabName ?? "???") + " is missing at " + zone.ToString());
      return false;
    }
    var clearRadius = location.m_location.m_location.m_clearArea ? location.m_location.m_exteriorRadius : 0f;
    if (Args.ObjectReset.HasValue) clearRadius = Args.ObjectReset.Value;
    SpawnLocation(zone, location, clearRadius);
    AddPin(location.m_position);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " spawned at " + zone.ToString());
    return true;
  }
}
