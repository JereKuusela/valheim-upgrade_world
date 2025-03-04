namespace UpgradeWorld;
public class CleanStandsCommand
{
  public CleanStandsCommand()
  {
    CommandWrapper.Register("clean_stands", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_stands", "[...args] - Removes missing objects from armor and item stands.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanStands(args.Context, zdos, pars.Pin, true);
    });
  }
}
