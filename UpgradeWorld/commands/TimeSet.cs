using System.Linq;
namespace UpgradeWorld;
public class TimeSetCommand {
  public TimeSetCommand() {
    CommandWrapper.Register("time_set", (int index) => {
      if (index == 0) return CommandWrapper.Info("The amount of seconds to set the time.");
      return null;
    });
    new Terminal.ConsoleCommand("time_set", "[seconds] - Sets the world time and updates object timestamps.", (Terminal.ConsoleEventArgs args) => {
      if (args.Args.Count() == 0) {
        Helper.Print(args.Context, "Error: Missing seconds.");
        return;
      }
      var time = Parse.Int(args[1], 0);
      if (time == 0) {
        Helper.Print(args.Context, "Error: Invalid format for seconds.");
        return;
      }
      if (args.Args.Count() > 2) {
        Helper.Print(args.Context, "Error: Too many parameters.");
        return;
      }
      if (!Helper.IsServer(args)) return;
      new SetTime(args.Context, time);
    });
    CommandWrapper.Register("time_set_day", (int index) => {
      if (index == 0) return CommandWrapper.Info("The day to set the time.");
      return null;
    });
    new Terminal.ConsoleCommand("time_set_day", "[days] - Sets the world time and updates object timestamps.", (Terminal.ConsoleEventArgs args) => {
      if (args.Args.Count() == 0) {
        Helper.Print(args.Context, "Error: Missing day.");
        return;
      }
      var time = Parse.Int(args[1], 0);
      if (time == 0) {
        Helper.Print(args.Context, "Error: Invalid format for day.");
        return;
      }
      if (args.Args.Count() > 2) {
        Helper.Print(args.Context, "Error: Too many parameters.");
        return;
      }
      if (!Helper.IsServer(args)) return;
      new SetTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
    });
  }
}
