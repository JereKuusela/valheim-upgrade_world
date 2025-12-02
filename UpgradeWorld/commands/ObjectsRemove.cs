namespace UpgradeWorld;

public class ObjectsRemoveCommand
{
  public ObjectsRemoveCommand()
  {
    CommandWrapper.Register("objects_remove", index =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("objects_remove", "[id1,id2,...] [...args] - Removes objects.", (args) =>
    {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveObjects(args.Context, pars.Ids(), pars), pars.Start);

    }, () => ZNetScene.instance.GetPrefabNames());
  }
}
