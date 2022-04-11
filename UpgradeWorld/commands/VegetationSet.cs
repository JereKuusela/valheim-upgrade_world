using System.Linq;
namespace UpgradeWorld;
public class VegetationSetCommands {
  public VegetationSetCommands() {
    CommandWrapper.Register("vegetation_enable", (int index) => SetVegetation.GetIds());
    new Terminal.ConsoleCommand("vegetation_enable", "[id1,id2,...] - Enables vegetation for the world generator.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var ids = args.Args.Skip(1);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: No ids given.");
        return;
      }
      new SetVegetation(args.Context, true, false, ids);
    }, optionsFetcher: SetVegetation.GetIds);
    CommandWrapper.Register("vegetation_disable", (int index) => SetVegetation.GetIds());
    new Terminal.ConsoleCommand("vegetation_disable", "[id1,id2,...] - Disables vegetation for the world generator.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var ids = args.Args.Skip(1);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: No ids given.");
        return;
      }
      new SetVegetation(args.Context, false, false, ids);
    }, optionsFetcher: SetVegetation.GetIds);
    CommandWrapper.RegisterEmpty("vegetation_default");
    new Terminal.ConsoleCommand("vegetation_default", "- Resets vegetation generation to the default.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      if (args.Length > 1) {
        args.Context.AddString("Error: No parameters expected.");
        return;
      }
      new ResetVegetation(args.Context);
    });
  }
}
