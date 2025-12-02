namespace UpgradeWorld;

public class WorldResetCommand
{
  public WorldResetCommand()
  {
    CommandWrapper.Register("world_reset", index =>
    {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    Helper.Command("world_reset", "[...args] - Resets zones and locations.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var ids = LocationOperation.AllIds();
      Executor.AddOperation(new RemoveLocations(args.Context, [.. LocationOperation.AllIds()], pars), false);
      Executor.AddOperation(new DistributeLocations(args.Context, [.. LocationOperation.AllIds()], pars), false);
      Executor.AddOperation(new ResetZones(args.Context, pars), pars.Start);
    }, () => FiltererParameters.Parameters);
  }
}
