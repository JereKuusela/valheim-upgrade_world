using System.Linq;
namespace UpgradeWorld;
public class RerollChestsCommand {
  public RerollChestsCommand() {
    new Terminal.ConsoleCommand("reroll_chests", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var extra = Parse.FiltererArgs(args.Args, parameters);
      extra = Parse.Flag(extra, "looted", out var looted);
      if (extra.Count() == 0) {
        args.Context.AddString("Error: Missing chest name.");
        return;
      }
      var chestName = extra.First();
      var ids = extra.Skip(1);
      new RerollChests(chestName, ids, looted, parameters, args.Context);
    }, optionsFetcher: () => RerollChests.ChestsNames);
  }
}
