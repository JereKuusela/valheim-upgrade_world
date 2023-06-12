namespace UpgradeWorld;
public class LocationsListCommand {
  public LocationsListCommand() {
    CommandWrapper.Register("locations_list", (int index) => {
      if (index == 0) return CommandWrapper.LocationIds();
      return DataParameters.Parameters;
    }, DataParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("locations_list", "[id1,id2,...] [...args] - Lists locations showing their position and biome.", (args) => {
      LocationIdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new ListLocationPositions(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
