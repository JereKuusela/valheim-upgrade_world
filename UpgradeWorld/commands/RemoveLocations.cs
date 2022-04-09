using System.Linq;
namespace UpgradeWorld;
public class RemoveLocationsCommand {
  public RemoveLocationsCommand() {
    new Terminal.ConsoleCommand("remove_locations", "[...location_ids] [...args] - Removes given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        Executor.AddOperation(new RemoveLocations(args.Context, pars.Ids, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
