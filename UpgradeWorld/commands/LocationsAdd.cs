using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationsAddCommand
{
  public LocationsAddCommand()
  {
    CommandWrapper.Register("locations_add", (int index) =>
    {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_add", "[id1,id2,...] [...args] - Adds missing locations to generated areas.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      Executor.AddOperation(new DistributeLocations(pars.Ids, pars.Start, pars.Chance, args.Context));
      Executor.AddOperation(new SpawnLocations(args.Context, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
