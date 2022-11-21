namespace UpgradeWorld;
///<summary>Adds new location instances.</summary>
public class AddLocations : ZoneOperation
{
  public AddLocations(Terminal context, FiltererParameters args) : base(context, args)
  {
    Operation = "Place missing locations";
    InitString = args.Print("Place missing locations to");
    args.Chance = 1f;
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    if (Attempts == 0 && !Args.Roll()) return true;
    var zoneSystem = ZoneSystem.instance;
    var locations = zoneSystem.m_locationInstances;
    if (zoneSystem.IsZoneLoaded(zone))
    {
      locations[zone] = new() { m_placed = false };
      return true;
    }
    zoneSystem.PokeLocalZone(zone);
    return false;
  }
}
