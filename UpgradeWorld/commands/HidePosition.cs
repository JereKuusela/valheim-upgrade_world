using UnityEngine;
namespace UpgradeWorld;
public class HidePositionCommand {
  public HidePositionCommand() {
    CommandWrapper.Register("hide_position", (int index) => {
      if (index == 0) return CommandWrapper.Info("X coordinate.");
      if (index == 1) return CommandWrapper.Info("Z coordinate.");
      if (index == 2) return CommandWrapper.Info("Distance.");
      return null;
    });
    new Terminal.ConsoleCommand("hide_position", "[x] [z] [distance=0] - Hides the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Parse.IncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new ExploreMap(position, distance, false, args.Context);
    }, isCheat: true);
  }
}
