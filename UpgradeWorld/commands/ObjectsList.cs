namespace UpgradeWorld;
public class ObjectsListCommand
{
  public ObjectsListCommand()
  {
    CommandWrapper.Register("objects_list", index =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("objects_list", "[id1,id2,...] [...args] - Lists objects showing their position and biome.", (args) =>
    {
      DataParameters pars = new(args, false);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new ListObjectPositions(args.Context, pars.Ids(), pars);
    }, () => ZNetScene.instance.GetPrefabNames());
  }
}
