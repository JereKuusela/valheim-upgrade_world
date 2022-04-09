using System.Linq;
namespace UpgradeWorld;
public class CountEntitiesCommand {
  public CountEntitiesCommand() {
    new Terminal.ConsoleCommand("count_entities", "[all] [...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var ids = Parse.FiltererArgs(args.Args, parameters);
      ids = Parse.Flag(ids, "all", out var showAll);
      if (ids.Count() == 0)
        new CountAllEntities(args.Context, showAll, parameters);
      else
        new CountEntities(args.Context, ids, parameters);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
