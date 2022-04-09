using System.Linq;
namespace UpgradeWorld;
public class RegenerateLocationsCommand {
  public RegenerateLocationsCommand() {
    new Terminal.ConsoleCommand("regenerate_locations", "[...location_ids] [...args] - Regenerates given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var ids = Parse.FiltererArgs(args.Args, parameters);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new RegenerateLocations(args.Context, ids, parameters));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
