using Service;

namespace UpgradeWorld;
public class UpgradeCommand {
  public UpgradeCommand() {
    CommandWrapper.Register("upgrade", (int index) => {
      if (index == 0) return Upgrade.Types;
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", (args) => {
      FiltererParameters pars = new(args);
      var selectedType = "";
      foreach (var type in Upgrade.Types) {
        if (Parse.Flag(pars.Unhandled, type.ToLower())) selectedType = type;
      }
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new Upgrade(args.Context, selectedType, pars.Unhandled, pars);
    }, optionsFetcher: () => Upgrade.Types);
  }
}
