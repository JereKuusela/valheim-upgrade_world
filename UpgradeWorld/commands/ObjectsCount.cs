namespace UpgradeWorld;
public class ObjectsCountCommand {
  public ObjectsCountCommand() {
    CommandWrapper.Register("objects_count", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return CountParameters.Parameters;
    }, CountParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_count", "[id1,id2,...] [...args] - Counts objects.", (args) => {
      CountParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CountObjects(args.Context, pars.Ids, pars, pars.Count);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
