using System.Linq;
namespace UpgradeWorld;
public class RegenerateLocationsCommand {
  public RegenerateLocationsCommand() {
    new Terminal.ConsoleCommand("regenerate_locations", "[...location_ids] [...args] - Regenerates given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        Executor.AddOperation(new RegenerateLocations(args.Context, pars.Ids, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
