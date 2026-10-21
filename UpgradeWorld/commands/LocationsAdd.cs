namespace UpgradeWorld;

public class LocationsAddCommand
{
  public LocationsAddCommand()
  {
    LocationOperation.RegisterExplored("locations_add");
    Helper.Command("locations_add", "[id1,id2,...] [...args] - Adds missing locations to generated areas.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      var spawn = pars.Explored != false;
      Executor.AddOperation(new DistributeLocations(args.Context, pars.Ids, pars, pars.Explored), !spawn && pars.Start);
      if (spawn)
        Executor.AddOperation(new SpawnLocations(args.Context, pars.Ids, pars), pars.Start);
    }, LocationOperation.AllIds);
  }
}
