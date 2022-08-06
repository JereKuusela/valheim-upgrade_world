namespace UpgradeWorld;
public class ObjectsListCommand {
  public ObjectsListCommand() {
    CommandWrapper.Register("objects_list", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_list", "[id1,id2,...] [...args] - Lists objects showing their position and biome.", (Terminal.ConsoleEventArgs args) => {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new ListObjectPositions(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
