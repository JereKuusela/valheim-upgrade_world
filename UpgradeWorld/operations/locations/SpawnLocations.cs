namespace UpgradeWorld;
///<summary>Spawns locations in explored areas.</summary>
public class SpawnLocations : LocationOperation
{
  private bool ClearLocationAreas = false;
  public SpawnLocations(Terminal context, bool clearLocationAreas, FiltererParameters args) : base(context, args)
  {
    Operation = "Spawn missing locations";
    InitString = args.Print("Spawn missing locations to");
    Verb = "spawned to the world";
    args.Chance = 1f;
    ClearLocationAreas = clearLocationAreas;
  }
  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location)
  {
    if (location.m_placed) return false;
    if (location.m_location?.m_prefab == null)
    {
      Print("Location " + (location.m_location?.m_prefabName ?? "???") + " is missing at " + zone.ToString());
      return false;
    }
    SpawnLocation(zone, location, ClearLocationAreas, false);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " spawned at " + zone.ToString());
    return true;
  }
}
