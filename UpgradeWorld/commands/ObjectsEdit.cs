namespace UpgradeWorld;

public class ObjectsEditCommand
{
  public ObjectsEditCommand()
  {
    CommandWrapper.Register("objects_edit", index =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("objects_edit", "[id1,id2,...] [...args] - Edits objects.", (args) =>
    {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new EditObjects(args.Context, pars.Ids(), pars));
    }, () => ZNetScene.instance.GetPrefabNames());
  }
}
