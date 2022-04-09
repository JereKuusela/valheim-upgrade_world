using UnityEngine;
namespace UpgradeWorld;
public class RemovePinsCommand {
  public RemovePinsCommand() {
    new Terminal.ConsoleCommand("remove_pins", "[x] [y] [distance=0] - Removes pins from the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Parse.IncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new RemovePins(position, distance, args.Context);
    }, isCheat: true);
  }
}
