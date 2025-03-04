namespace UpgradeWorld;
public class ObjetsRefreshCommand
{
  public ObjetsRefreshCommand()
  {
    CommandWrapper.Register("objects_refresh", index =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("objects_refresh", "[id1,id2,...] [...args] - Refreshes / respawn objects.", (args) =>
    {
      DataParameters pars = new(args, false);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new RefreshObjects(args.Context, pars.Ids(), pars);
    }, () => ZNetScene.instance.GetPrefabNames());
  }
}
