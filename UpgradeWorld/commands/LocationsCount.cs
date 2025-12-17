namespace UpgradeWorld;

public class LocationsCountCommand
{
  public LocationsCountCommand()
  {
    LocationOperation.Register("locations_count");
    Helper.Command("locations_count", "[id1,id2,...] [...args] - Counts locations and returns totals for each location type.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CountLocations(args.Context, pars.Ids, pars.Log, pars);
    }, LocationOperation.AllIds);
  }
}
