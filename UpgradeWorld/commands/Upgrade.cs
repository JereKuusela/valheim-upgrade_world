namespace UpgradeWorld;
public class UpgradeCommand {
  public UpgradeCommand() {
    new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var extra = Parse.FiltererArgs(args.Args, parameters);
      var selectedType = "";
      foreach (var type in Upgrade.Types) {
        extra = Parse.Flag(extra, type, out var found);
        if (found) selectedType = type;
      }
      new Upgrade(args.Context, selectedType, extra, parameters);
    }, optionsFetcher: () => Upgrade.Types);
  }
}
