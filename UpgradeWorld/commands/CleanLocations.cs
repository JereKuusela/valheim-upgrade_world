namespace UpgradeWorld;
public class CleanLocationsCommand
{
  public CleanLocationsCommand()
  {
    CommandWrapper.Register("clean_locations", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("clean_locations", "[...args] - Removes missing locations.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      var zdos = EntityOperation.GetZDOs(pars);
      new CleanLocations(args.Context, zdos, pars.Pin);
    });
  }
}
