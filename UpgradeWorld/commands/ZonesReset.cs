namespace UpgradeWorld;
public class ZonesResetCommand {
  public ZonesResetCommand() {
    CommandWrapper.Register("zones_reset", (int index) => {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("zones_reset", "[...args] - Destroys areas making them ungenerated. These areas will be generated when visited.", (Terminal.ConsoleEventArgs args) => {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new ResetZones(args.Context, pars));
    }, optionsFetcher: () => FiltererParameters.Parameters);
  }
}
