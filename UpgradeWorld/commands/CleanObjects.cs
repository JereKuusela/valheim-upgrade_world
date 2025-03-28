namespace UpgradeWorld;
public class CleanObjectsCommand
{
  public CleanObjectsCommand()
  {
    CommandWrapper.Register("clean_objects", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_objects", "[...args] - Removes missing objects.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanObjects(args.Context, zdos, pars.Pin, true);
    });
  }
}
