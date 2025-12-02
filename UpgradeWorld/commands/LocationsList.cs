namespace UpgradeWorld;

public class LocationsListCommand
{
  public LocationsListCommand()
  {
    LocationOperation.Register("locations_list");
    Helper.Command("locations_list", "[id1,id2,...] [...args] - Lists locations showing their position and biome.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new ListLocationPositions(args.Context, pars.Ids, pars.Log, pars);
    }, LocationOperation.AllIds);
  }
}
