using UnityEngine;
namespace UpgradeWorld;
public class DestroyCommand {
  public DestroyCommand() {
    new Terminal.ConsoleCommand("destroy", "[...args] - Destroys zones which allows the world generator to regenerate them.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var extra = Parse.FiltererArgs(args.Args, parameters);
      if (Helper.CheckUnhandled(args, extra))
        Executor.AddOperation(new Destroy(args.Context, parameters));
    }, optionsFetcher: () => Helper.AvailableBiomes);
  }
}
