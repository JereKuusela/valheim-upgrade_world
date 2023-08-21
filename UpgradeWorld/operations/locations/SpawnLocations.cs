namespace UpgradeWorld;
///<summary>Spawns locations in explored areas.</summary>
public class SpawnLocations : LocationOperation
{
  public SpawnLocations(Terminal context, FiltererParameters args) : base(context, args)
  {
    Operation = "Spawn missing locations";
    InitString = args.Print("Spawn missing locations to");
    Verb = "spawned to already generated areas";
    args.Chance = 1f;
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
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " spawned at " + zone.ToString());
    return true;
  }
}
