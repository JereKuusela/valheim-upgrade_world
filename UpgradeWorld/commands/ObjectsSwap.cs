namespace UpgradeWorld;
public class ObjectsSwapCommand {
  public ObjectsSwapCommand() {
    CommandWrapper.Register("objects_swap", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_swap", "[new id,id1,id2,...] [...args] - Swaps objects to a different one.", (args) => {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new SwapObjects(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
