namespace UpgradeWorld;
public class ListEntitiesCommand {
  public ListEntitiesCommand() {
    CommandWrapper.Register("list_entities", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("list_entities", "[...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        new ListEntityPositions(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
