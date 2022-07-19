namespace UpgradeWorld;
public class ZonesRestoreCommand {
  public ZonesRestoreCommand() {
    CommandWrapper.Register("zones_restore", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("zones_restore", "[...args] - Restores missing zone control objects.", (Terminal.ConsoleEventArgs args) => {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RestoreZones(args.Context, pars));
    });
  }
}
