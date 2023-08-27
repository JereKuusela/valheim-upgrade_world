namespace UpgradeWorld;
public class WorldResetCommand
{
  public WorldResetCommand()
  {
    CommandWrapper.Register("world_reset", (int index) =>
    {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("world_reset", "[...args] - Resets zones and locations.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveLocations(args.Context, [], pars));
      Executor.AddOperation(new DistributeLocations(args.Context, [], pars.Start, pars.Chance));
      Executor.AddOperation(new ResetZones(args.Context, pars));
    }, optionsFetcher: () => FiltererParameters.Parameters);
  }
}
