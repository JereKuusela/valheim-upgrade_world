namespace UpgradeWorld;
public class CleanDungeonsCommand
{
  public CleanDungeonsCommand()
  {
    CommandWrapper.Register("clean_dungeons", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_dungeons", "[...args] - Optimizes old dungeons.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanDungeons(args.Context, zdos, pars.Pin, true);
    });
  }
}
