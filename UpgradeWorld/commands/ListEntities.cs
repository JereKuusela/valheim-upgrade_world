namespace UpgradeWorld;
public class ListEntitiesCommand {
  public ListEntitiesCommand() {
    new Terminal.ConsoleCommand("list_entities", "[...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var ids = Parse.FiltererArgs(args.Args, parameters);
      new ListEntityPositions(args.Context, ids, parameters);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
