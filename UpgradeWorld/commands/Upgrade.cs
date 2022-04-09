using System.Linq;
namespace UpgradeWorld;
public class UpgradeCommand {
  public UpgradeCommand() {
    new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters pars = new(args);
      var selectedType = "";
      foreach (var type in Upgrade.Types) {
        pars.Unhandled = Parse.Flag(pars.Unhandled, type, out var found).ToList();
        if (found) selectedType = type;
      }
      new Upgrade(args.Context, selectedType, pars.Unhandled, pars);
    }, optionsFetcher: () => Upgrade.Types);
  }
}
