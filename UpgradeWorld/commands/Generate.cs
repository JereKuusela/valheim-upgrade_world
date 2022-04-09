using UnityEngine;
namespace UpgradeWorld;
public class GenerateCommand {
  public GenerateCommand() {
    CommandWrapper.Register("generate", (int index) => {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("generate", "[...args] - Generates zones which allows pregenerating the world without having to move there physically.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters pars = new(args);
      pars.TargetZones = TargetZones.Ungenerated;
      pars.SafeZones = 0;
      if (pars.Valid(args.Context))
        Executor.AddOperation(new Generate(args.Context, pars));
    }, optionsFetcher: () => Helper.AvailableBiomes);
  }
}
