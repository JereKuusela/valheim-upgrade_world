using System.Linq;
namespace UpgradeWorld;
public class VegetationAddCommand {
  public VegetationAddCommand() {
    CommandWrapper.Register("vegetation_add", (int index) => {
      if (index == 0) return SetVegetation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("vegetation_add", "[id1,id2,...] [...args] - Adds vegetation to generated areas.", (Terminal.ConsoleEventArgs args) => {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new PlaceVegetation(args.Context, pars.Ids.ToHashSet(), pars));
    }, optionsFetcher: () => SetVegetation.GetIds());
  }
}
