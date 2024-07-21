namespace UpgradeWorld;
public class CleanChestsCommand
{
  public CleanChestsCommand()
  {
    CommandWrapper.Register("clean_chests", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("clean_chests", "[...args] - Removes missing objects from chests.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanChests(args.Context, zdos, pars.Pin);
    });
  }
}
