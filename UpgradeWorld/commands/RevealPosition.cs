using UnityEngine;
namespace UpgradeWorld;
public class RevealPositionCommand {
  public RevealPositionCommand() {
    new Terminal.ConsoleCommand("reveal_position", "[x] [y] [distance=0] - Explores the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Parse.IncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new ExploreMap(position, distance, true, args.Context);
    }, isCheat: true);
  }
}
