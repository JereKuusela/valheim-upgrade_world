namespace UpgradeWorld;
public class ObjectsListCommand {
  public ObjectsListCommand() {
    CommandWrapper.Register("objects_list", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_list", "[id1,id2,...] [...args] - Lists objects showing their position and biome.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        new ListObjectPositions(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
