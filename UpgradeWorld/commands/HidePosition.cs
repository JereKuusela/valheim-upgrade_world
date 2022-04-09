using UnityEngine;
namespace UpgradeWorld;
public class HidePositionCommand {
  public HidePositionCommand() {
    new Terminal.ConsoleCommand("hide_position", "[x] [y] [distance=0] - Hides the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Parse.IncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new ExploreMap(position, distance, false, args.Context);
    }, isCheat: true);
  }
}
