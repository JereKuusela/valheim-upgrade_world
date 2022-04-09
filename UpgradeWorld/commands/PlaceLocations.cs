using System.Linq;
namespace UpgradeWorld;
public class PlaceLocationsCommand {
  public PlaceLocationsCommand() {
    CommandWrapper.Register("place_locations", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("place_locations", "[...location_ids] [noclearing] [...args] - Places given location ids to already generated zones.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "noclearing", out var noClearing).ToList();
      if (!pars.Valid(args.Context)) return;
      Executor.AddOperation(new DistributeLocations(pars.Ids, pars.ForceStart, args.Context));
      Executor.AddOperation(new PlaceLocations(args.Context, !noClearing, pars));
    }, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
  }
}
