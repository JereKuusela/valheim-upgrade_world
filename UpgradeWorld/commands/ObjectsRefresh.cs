namespace UpgradeWorld;
public class ObjetsRefreshCommand
{
  public ObjetsRefreshCommand()
  {
    CommandWrapper.Register("objects_refresh", (int index) =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_refresh", "[id1,id2,...] [...args] - Refreshes / respawn objects.", (args) =>
    {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new RefreshObjects(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
