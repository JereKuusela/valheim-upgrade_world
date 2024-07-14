namespace UpgradeWorld;
public class LocationsCleanCommand
{
  public LocationsCleanCommand()
  {
    CommandWrapper.Register("locations_clean", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_clean", "[...args] - Removes missing locations.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CleanLocations(args.Context, pars);
    });
  }
}
