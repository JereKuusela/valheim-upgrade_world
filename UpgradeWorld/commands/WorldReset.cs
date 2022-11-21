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
      Executor.AddOperation(new RemoveLocations(args.Context, new string[0], pars));
      Executor.AddOperation(new DistributeLocations(new string[0], pars.Start, pars.Chance, args.Context));
      Executor.AddOperation(new ResetZones(args.Context, pars));
    }, optionsFetcher: () => FiltererParameters.Parameters);
  }
}
