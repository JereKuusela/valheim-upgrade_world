namespace UpgradeWorld;
public class WorldCleanCommand {
  public WorldCleanCommand() {
    CommandWrapper.Register("world_clean", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("world_clean", "[...args] - Removes missing objects.", (args) => {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CleanObjects(args.Context, pars);
    });
  }
}
