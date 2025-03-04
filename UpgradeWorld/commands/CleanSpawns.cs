namespace UpgradeWorld;
public class CleanSpawnsCommand
{
  public CleanSpawnsCommand()
  {
    CommandWrapper.Register("clean_spawns", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_spawns", "[...args] - Removes timtestamps from the spawn system.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanSpawns(args.Context, zdos, pars.Pin, true);
    });
  }
}
