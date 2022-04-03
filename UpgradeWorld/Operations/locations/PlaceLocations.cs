namespace UpgradeWorld;
///<summary>Places all unplaced locations.</summary>
public class PlaceLocations : LocationOperation {
  private bool ClearLocationAreas = false;
  public PlaceLocations(Terminal context, bool clearLocationAreas, FiltererParameters args) : base(context, args) {
    Operation = "Place locations";
    InitString = args.Print("Place locations at");
    Verb = "placed";
    ClearLocationAreas = clearLocationAreas;
  }
  protected override bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location) {
    if (location.m_placed) return false;
    PlaceLocation(zone, location, ClearLocationAreas, false);
    if (Settings.Verbose)
      Print("Location " + location.m_location.m_prefabName + " removed at " + zone.ToString());
    return true;
  }
}
