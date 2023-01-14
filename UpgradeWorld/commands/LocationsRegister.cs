using System.Linq;

namespace UpgradeWorld;
public class LocationsRegisterCommand
{
  public LocationsRegisterCommand()
  {
    CommandWrapper.Register("locations_register", (int index) =>
    {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_register", "[id] [...args] - Registers locations without placing them.", (args) =>
    {
      LocationIdParameters pars = new(args);
      pars.SafeZones = 0;
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      Executor.AddOperation(new RegisterLocations(args.Context, pars.Ids.FirstOrDefault(), pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
