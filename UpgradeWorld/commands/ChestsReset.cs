using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class ChestsResetCommand
{
  public ChestsResetCommand()
  {
    CommandWrapper.Register("chests_reset", index =>
    {
      if (index == 0) return ResetChests.ChestNames();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("chests_reset", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (args) =>
    {
      DataParameters pars = new(args, false);
      var looted = Parse.Flag(pars.Unhandled, "looted");
      if (!pars.Valid(args.Context)) return;
      var chestIds = ResetChests.ChestNames().ToArray();
      HashSet<string> ids = [];
      if (pars.Ids().Count() > 0)
      {
        chestIds = [pars.Ids().First()];
        ids = [.. pars.Ids().Skip(1)];
      }
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new ResetChests(chestIds, ids, looted, pars, args.Context), pars.Start);
    }, ResetChests.ChestNames);
  }
}
