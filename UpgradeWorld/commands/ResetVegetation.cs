using System.Linq;
namespace UpgradeWorld;
public class ResetVegetationCommand {
  public ResetVegetationCommand() {
    CommandWrapper.Register("reset_vegetation", (int index) => {
      if (index == 0) return SetVegetation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("reset_vegetation", "[...vegetation_ids] [...args] - Removes and places given vegetation ids to already generated zones.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context)) {
        Executor.AddOperation(new RemoveVegetation(args.Context, pars.Ids.ToHashSet(), pars));
        Executor.AddOperation(new PlaceVegetation(args.Context, pars.Ids.ToHashSet(), pars));
      }
    }, optionsFetcher: () => SetVegetation.GetIds());
  }
}
