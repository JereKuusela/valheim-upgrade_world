namespace UpgradeWorld;
public class WorldCleanCommand
{
  public WorldCleanCommand()
  {
    CommandWrapper.Register("world_clean", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("world_clean", "[...args] - Removes missing locations and objects.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CleanLocations(args.Context, pars);
      new CleanObjects(args.Context, pars);
    });
  }
}
