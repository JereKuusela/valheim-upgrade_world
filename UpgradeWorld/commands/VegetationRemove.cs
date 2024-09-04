namespace UpgradeWorld;
public class VegetationRemoveCommand
{
  public VegetationRemoveCommand()
  {
    CommandWrapper.Register("vegetation_remove", (int index) =>
    {
      if (index == 0) return VegetationOperation.GetIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("vegetation_remove", "[id1,id2,...] [...args] - Removes vegetation from generated areas.", (args) =>
    {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveVegetation(args.Context, [.. pars.Ids], pars));
    }, optionsFetcher: VegetationOperation.GetIds);
  }
}
