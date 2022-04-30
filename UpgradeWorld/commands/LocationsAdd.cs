using System.Linq;
namespace UpgradeWorld;
public class LocationsAddCommand {
  public LocationsAddCommand() {
    CommandWrapper.Register("locations_add", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_add", "[...location_ids] [noclearing] [...args] - Adds missing locations to generated areas.", (Terminal.ConsoleEventArgs args) => {
      RequiredIdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "noclearing", out var noClearing).ToList();
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new DistributeLocations(pars.Ids, pars.Start, pars.Chance, args.Context));
      Executor.AddOperation(new PlaceLocations(args.Context, !noClearing, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
