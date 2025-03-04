namespace UpgradeWorld;
public class CleanHealthCommand
{
  public CleanHealthCommand()
  {
    CommandWrapper.Register("clean_health", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_health", "[...args] - Removes excess health data from creatures and structures.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanHealth(args.Context, zdos, pars.Pin, true);
    });
  }
}
