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
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanLocations(args.Context, zdos, pars.Pin);
      new CleanObjects(args.Context, zdos, pars.Pin);
      new CleanChests(args.Context, zdos, pars.Pin);
      new CleanStands(args.Context, zdos, pars.Pin);
      new CleanDungeons(args.Context, zdos, pars.Pin);
      new CleanSpawns(args.Context, zdos, pars.Pin);
      new CleanHealth(args.Context, zdos, pars.Pin);
    });
  }
}
