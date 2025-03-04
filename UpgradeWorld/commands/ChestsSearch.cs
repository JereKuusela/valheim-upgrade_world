namespace UpgradeWorld;
public class ChestsSearchCommand
{
  public ChestsSearchCommand()
  {
    CommandWrapper.Register("chests_search", index =>
    {
      if (index == 0) return CommandWrapper.ObjectIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    Helper.Command("chests_search", "[id1,id2,...] [...args] - Searchs contents of chests and stands.", (args) =>
    {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new SearchChests(args.Context, pars.Ids(), pars);
    }, () => ZNetScene.instance.GetPrefabNames());
  }
}
