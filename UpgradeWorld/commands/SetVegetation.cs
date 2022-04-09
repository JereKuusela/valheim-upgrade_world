using System.Linq;
namespace UpgradeWorld;
public class SetVegetationCommand {
  public SetVegetationCommand() {
    CommandWrapper.Register("set_vegetation", (int index) => SetVegetation.GetIds());
    new Terminal.ConsoleCommand("set_vegetation", "[...ids] [disable] - Enables/disables vegetation for the world generator.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var ids = args.Args.Skip(1);
      ids = Parse.Flag(args.Args, "disable", out var disable);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: No entity ids given.");
        return;
      }
      new SetVegetation(args.Context, !disable, ids);
    }, optionsFetcher: SetVegetation.GetIds);
    CommandWrapper.RegisterEmpty("reset_vegetation");
    new Terminal.ConsoleCommand("reset_vegetation", "- Resets vegetation generation to the default.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      if (args.Length > 1) {
        args.Context.AddString("Error: No parameters expected.");
        return;
      }
      new ResetVegetation(args.Context);
    });
  }
}
