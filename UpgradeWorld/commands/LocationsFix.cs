namespace UpgradeWorld;

public class LocationsFixCommand
{
  public LocationsFixCommand()
  {
    CommandWrapper.Register("locations_fix", (index, subIndex) => null);
    Helper.Command("locations_fix", "- Fixes missing location registrations.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      new FixLocations(args.Context, pars);
    });
  }
}
