using System.Linq;
namespace UpgradeWorld;
public class LocationsRemoveCommand {
  public LocationsRemoveCommand() {
    CommandWrapper.Register("locations_remove", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_remove", "[id1,id2,...] [...args] -  Removes locations and prevents new ones from appearing (until a command like 'genloc' or 'locations_add' is used).", (Terminal.ConsoleEventArgs args) => {
      IdParameters pars = new(args, true);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveLocations(args.Context, pars.Ids, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
