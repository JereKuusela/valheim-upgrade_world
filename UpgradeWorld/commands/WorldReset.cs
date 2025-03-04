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
      Executor.AddOperation(new RemoveLocations(args.Context, [], pars));
      Executor.AddOperation(new DistributeLocations(args.Context, [], pars));
      Executor.AddOperation(new ResetZones(args.Context, pars));
    }, () => FiltererParameters.Parameters);
  }
}
