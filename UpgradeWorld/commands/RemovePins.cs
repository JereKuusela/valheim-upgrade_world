using UnityEngine;
namespace UpgradeWorld;
public class RemovePinsCommand {
  public RemovePinsCommand() {
    CommandWrapper.Register("remove_pins", (int index) => {
      if (index == 0) return CommandWrapper.Info("X coordinate.");
      if (index == 1) return CommandWrapper.Info("Z coordinate.");
      if (index == 2) return CommandWrapper.Info("Distance.");
      return null;
    });
    new Terminal.ConsoleCommand("remove_pins", "[x] [z] [distance=0] - Removes pins from the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Parse.IncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new RemovePins(position, distance, args.Context);
    }, isCheat: true);
  }
}
