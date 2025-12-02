using System.Linq;

namespace UpgradeWorld;

public class LocationsAddCommand
{
  public LocationsAddCommand()
  {
    LocationOperation.Register("locations_add");
    Helper.Command("locations_add", "[id1,id2,...] [...args] - Adds missing locations to generated areas.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      Executor.AddOperation(new DistributeLocations(args.Context, pars.Ids, pars), false);
      Executor.AddOperation(new SpawnLocations(args.Context, pars.Ids, pars), pars.Start);
    }, LocationOperation.AllIds);
  }
}
