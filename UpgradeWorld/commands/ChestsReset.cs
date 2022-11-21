using System.Linq;
using Service;

namespace UpgradeWorld;
public class ChestsResetCommand
{
  public ChestsResetCommand()
  {
    CommandWrapper.Register("chests_reset", (int index) =>
    {
      if (index == 0) return ResetChests.ChestNames();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("chests_reset", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (args) =>
    {
      DataParameters pars = new(args, true);
      pars.Ids = Parse.Flag(pars.Ids, "looted", out var looted).ToList();
      var chestIds = ResetChests.ChestNames().ToArray();
      if (pars.Ids.Count() > 0)
      {
        chestIds = new string[] { pars.Ids.First() };
        pars.Ids = pars.Ids.Skip(1).ToList();
      }
      if (Helper.IsClient(args)) return;
      new ResetChests(chestIds, pars.Ids, looted, pars, args.Context);
    }, optionsFetcher: ResetChests.ChestNames);
  }
}
