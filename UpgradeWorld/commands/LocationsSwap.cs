using System.Linq;

namespace UpgradeWorld;
public class LocationsSwapCommand
{
  public LocationsSwapCommand()
  {
    CommandWrapper.Register("locations_swap", (int index) =>
    {
      if (index == 0) return CommandWrapper.LocationIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_swap", "[new id,id1,id2,...] [...args] - Swaps locations to a different one.", (args) =>
    {
      DataParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new SwapLocations(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
