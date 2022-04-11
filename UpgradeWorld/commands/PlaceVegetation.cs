using System.Linq;
namespace UpgradeWorld;
public class PlaceVegetationCommand {
  public PlaceVegetationCommand() {
    CommandWrapper.Register("place_vegetation", (int index) => {
      if (index == 0) return SetVegetation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("place_vegetation", "[...vegetation_ids] [...args] - Places given vegetation ids to already generated zones.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        Executor.AddOperation(new PlaceVegetation(args.Context, pars.Ids.ToHashSet(), pars));
    }, optionsFetcher: () => SetVegetation.GetIds());
  }
}
