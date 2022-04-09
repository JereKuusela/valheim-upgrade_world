using System.Linq;
namespace UpgradeWorld;
public class RerollChestsCommand {
  public RerollChestsCommand() {
    CommandWrapper.Register("reroll_chests", (int index) => {
      if (index == 0) return RerollChests.ChestsNames;
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("reroll_chests", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "looted", out var looted).ToList();
      if (pars.Ids.Count() == 0) {
        args.Context.AddString("Error: Missing chest name.");
        return;
      }
      var chestName = pars.Ids.First();
      var ids = pars.Ids.Skip(1);
      new RerollChests(chestName, ids, looted, pars, args.Context);
    }, optionsFetcher: () => RerollChests.ChestsNames);
  }
}
