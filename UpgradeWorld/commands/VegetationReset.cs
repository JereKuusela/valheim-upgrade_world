using System.Linq;
namespace UpgradeWorld;
public class VegetationResetCommand {
  public VegetationResetCommand() {
    CommandWrapper.Register("vegetation_reset", (int index) => {
      if (index == 0) return SetVegetation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("vegetation_reset", "[id1,id2,...] [...args] - Removes and adds vegetation to generated areas.", (Terminal.ConsoleEventArgs args) => {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (!Helper.IsServer(args)) return;
      Executor.AddOperation(new RemoveVegetation(args.Context, pars.Ids.ToHashSet(), pars));
      Executor.AddOperation(new PlaceVegetation(args.Context, pars.Ids.ToHashSet(), pars));
    }, optionsFetcher: () => SetVegetation.GetIds());
  }
}
