using UnityEngine;
namespace UpgradeWorld;
public class DestroyCommand {
  public DestroyCommand() {
    new Terminal.ConsoleCommand("destroy", "[...args] - Destroys zones which allows the world generator to regenerate them.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters pars = new(args);
      if (pars.Valid(args.Context))
        Executor.AddOperation(new Destroy(args.Context, pars));
    }, optionsFetcher: () => Helper.AvailableBiomes);
  }
}
