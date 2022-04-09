using System.Linq;
namespace UpgradeWorld;
public class SetDayCommand {
  public SetDayCommand() {
    new Terminal.ConsoleCommand("set_day", "[day] - Changes day while updating entities.", (Terminal.ConsoleEventArgs args) => {
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
      new SetTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
    });
  }
}
