using System.Linq;
namespace UpgradeWorld;
public class SetTimeCommand {
  public SetTimeCommand() {
    new Terminal.ConsoleCommand("set_time", "[seconds] - Changes time while updating entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing seconds.");
        return;
      }
      var time = Parse.Int(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for seconds.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new SetTime(args.Context, time);
    });
  }
}
