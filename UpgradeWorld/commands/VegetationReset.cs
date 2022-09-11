using System.Linq;
namespace UpgradeWorld;
public class VegetationResetCommand {
  public VegetationResetCommand() {
    CommandWrapper.Register("vegetation_reset", (int index) => {
      if (index == 0) return VegetationOperation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("vegetation_reset", "[id1,id2,...] [...args] - Removes and adds vegetation to generated areas.", (args) => {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveVegetation(args.Context, pars.Ids.ToHashSet(), pars));
      Executor.AddOperation(new AddVegetation(args.Context, pars.Ids.ToHashSet(), pars));
    }, optionsFetcher: VegetationOperation.GetIds);
  }
}
