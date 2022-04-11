using System.Linq;
namespace UpgradeWorld;
public class LocationsResetCommand {
  public LocationsResetCommand() {
    CommandWrapper.Register("locations_reset", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_reset", "[id1,id2,...] [...args] - Resets locations by removing them and then placing them at the same position. Dungeons which have a random rotation will also get a new layout.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        Executor.AddOperation(new RegenerateLocations(args.Context, pars.Ids, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
