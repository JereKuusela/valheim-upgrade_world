using System.Linq;
namespace UpgradeWorld;
public class TimeChangeCommand {
  public TimeChangeCommand() {
    CommandWrapper.Register("time_change", (int index) => {
      if (index == 0) return CommandWrapper.Info("Amount of seconds to go forward or backward.");
      return null;
    });
    new Terminal.ConsoleCommand("time_change", "[seconds] - Changes the world time and updates object timestamps.", (Terminal.ConsoleEventArgs args) => {
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
    CommandWrapper.Register("time_change_day", (int index) => {
      if (index == 0) return CommandWrapper.Info("Amount of days to go forward or backward.");
      return null;
    });
    new Terminal.ConsoleCommand("time_change_day", "[days] - Changes the world time and updates object timestamps.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing day.");
        return;
      }
      var time = Parse.Int(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for day.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new ChangeTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
    });
  }
}
