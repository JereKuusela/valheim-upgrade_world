namespace UpgradeWorld;
public class ZonesGenerateCommand {
  public ZonesGenerateCommand() {
    CommandWrapper.Register("zones_generate", (int index) => {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("zones_generate", "[...args] - Pre-generates areas without having to visit them.", (Terminal.ConsoleEventArgs args) => {
      FiltererParameters pars = new(args);
      pars.TargetZones = TargetZones.Ungenerated;
      pars.SafeZones = 0;
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new Generate(args.Context, pars));
    }, optionsFetcher: () => Helper.AvailableBiomes);
  }
}
