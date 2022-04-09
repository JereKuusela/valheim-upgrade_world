using System.Linq;
namespace UpgradeWorld;
public class RemoveLocationsCommand {
  public RemoveLocationsCommand() {
    new Terminal.ConsoleCommand("remove_locations", "[...location_ids] [...args] - Removes given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var ids = Parse.FiltererArgs(args.Args, parameters);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new RemoveLocations(args.Context, ids, parameters));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
