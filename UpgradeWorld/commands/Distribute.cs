using UnityEngine;
namespace UpgradeWorld;
public class DistributeCommand {
  public DistributeCommand() {
    new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      Executor.AddOperation(new DistributeLocations(new string[0], true, args.Context));
    });
  }
}
