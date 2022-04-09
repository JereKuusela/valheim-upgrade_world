using System.Linq;
namespace UpgradeWorld;
public class CountEntitiesCommand {
  public CountEntitiesCommand() {
    new Terminal.ConsoleCommand("count_entities", "[all] [...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "all", out var showAll).ToList();
      if (!pars.Valid(args.Context)) return;
      if (pars.Ids.Count() == 0)
        new CountAllEntities(args.Context, showAll, pars);
      else
        new CountEntities(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
