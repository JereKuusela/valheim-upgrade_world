using UnityEngine;
namespace UpgradeWorld;
public class GenerateCommand {
  public GenerateCommand() {
    new Terminal.ConsoleCommand("generate", "[...args] - Generates zones which allows pregenerating the world without having to move there physically.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var extra = Parse.FiltererArgs(args.Args, parameters);
      parameters.TargetZones = TargetZones.Ungenerated;
      parameters.SafeZones = 0;
      if (Helper.CheckUnhandled(args, extra))
        Executor.AddOperation(new Generate(args.Context, parameters));
    }, optionsFetcher: () => Helper.AvailableBiomes);
  }
}
