using System.Linq;
namespace UpgradeWorld;
public class ChangeTimeCommand {
  public ChangeTimeCommand() {
    new Terminal.ConsoleCommand("change_time", "[seconds] - Changes time while updating entities.", (Terminal.ConsoleEventArgs args) => {
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
      new ChangeTime(args.Context, time);
    });
  }
}
