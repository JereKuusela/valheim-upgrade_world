using System.Linq;
namespace UpgradeWorld;
public class ChestsResetCommand {
  public ChestsResetCommand() {
    CommandWrapper.Register("chests_reset", (int index) => {
      if (index == 0) return ResetChests.ChestsNames;
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("chests_reset", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (Terminal.ConsoleEventArgs args) => {
      IdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "looted", out var looted).ToList();
      if (pars.Ids.Count() == 0) {
        Helper.Print(args.Context, "Error: Missing chest name.");
        return;
      }
      var chestName = pars.Ids.First();
      var ids = pars.Ids.Skip(1);
      if (!Helper.IsServer(args)) return;
      new ResetChests(chestName, ids, looted, pars, args.Context);
    }, optionsFetcher: () => ResetChests.ChestsNames);
  }
}
