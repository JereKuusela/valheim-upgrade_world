namespace UpgradeWorld;
public class ObjectsRemoveCommand {
  public ObjectsRemoveCommand() {
    CommandWrapper.Register("objects_Remove", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_Remove", "[id1,id2,...] [...args] - Removes objects.", (Terminal.ConsoleEventArgs args) => {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new RemoveObjects(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
