namespace UpgradeWorld;

public class LocationsRestoreCommand
{
  public LocationsRestoreCommand()
  {
    LocationOperation.Register("locations_restore", "explored");
    Helper.Command("locations_restore", "[id1,id2,...] [...args] - Moves locations to the places they would have in a fresh world.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      var explored = pars.Explored == true;
      Executor.AddOperation(new RestoreLocations(args.Context, pars.Ids, pars, explored), !explored && pars.Start);
      if (explored)
        Executor.AddOperation(new SpawnLocations(args.Context, pars.Ids, pars), pars.Start);
    }, LocationOperation.AllIds);
  }
}
