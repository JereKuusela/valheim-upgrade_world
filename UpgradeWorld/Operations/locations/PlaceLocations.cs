namespace UpgradeWorld;
///<summary>Places all unplaced locations.</summary>
public class PlaceLocations : LocationOperation
{
  private bool ClearLocationAreas = false;
  public PlaceLocations(Terminal context, bool clearLocationAreas, FiltererParameters args) : base(context, args)
  {
    Operation = "Place missing locations";
    InitString = args.Print("Place missing locations to");
    Verb = "placed";
    args.Chance = 1f;
    ClearLocationAreas = clearLocationAreas;
  }
  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location)
  {
    if (location.m_placed) return false;
    PlaceLocation(zone, location, ClearLocationAreas, false);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " placed at " + zone.ToString());
    return true;
  }
}
