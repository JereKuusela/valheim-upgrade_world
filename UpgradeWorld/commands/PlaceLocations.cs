using System.Linq;
namespace UpgradeWorld;
public class PlaceLocationsCommand {
  public PlaceLocationsCommand() {
    new Terminal.ConsoleCommand("place_locations", "[...location_ids] [noclearing] [...args] - Places given location ids to already generated zones.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var extra = Parse.FiltererArgs(args.Args, parameters);
      var ids = Parse.Flag(extra, "noclearing", out var noClearing);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new DistributeLocations(ids, parameters.ForceStart, args.Context));
      Executor.AddOperation(new PlaceLocations(args.Context, !noClearing, parameters));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
