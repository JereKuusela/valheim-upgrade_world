namespace UpgradeWorld;
public class ObjectsEditCommand {
  public ObjectsEditCommand() {
    CommandWrapper.Register("objects_edit", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_edit", "[id1,id2,...] [...args] - Edits objects.", (args) => {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new EditObjects(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
