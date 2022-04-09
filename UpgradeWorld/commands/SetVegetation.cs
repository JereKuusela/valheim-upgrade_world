using System.Linq;
namespace UpgradeWorld;
public class SetVegetationCommand {
  public SetVegetationCommand() {
    new Terminal.ConsoleCommand("set_vegetation", "[...ids] [disable] - Enables/disables vegetation for the world generator.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var ids = Parse.Args(args.Args, 1);
      ids = Parse.Flag(args.Args, "disable", out var disable);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: No entity ids given.");
        return;
      }
      new SetVegetation(args.Context, !disable, ids);
    }, optionsFetcher: SetVegetation.GetIds);
    new Terminal.ConsoleCommand("reset_vegetation", "- Resets vegetation generation to the default.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var extra = Parse.Args(args.Args, 1);
      if (extra.Count() > 0) {
        args.Context.AddString("Error: No parameters expected.");
        return;
      }
      new ResetVegetation(args.Context);
    });
  }
}
