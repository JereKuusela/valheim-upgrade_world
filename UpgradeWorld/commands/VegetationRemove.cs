namespace UpgradeWorld;
public class VegetationRemoveCommand
{
  public VegetationRemoveCommand()
  {
    VegetationOperation.Register("vegetation_remove");
    Helper.Command("vegetation_remove", "[id1,id2,...] [...args] - Removes vegetation from generated areas.", (args) =>
    {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RemoveVegetation(args.Context, pars.VegIds(), pars));
    }, VegetationOperation.AllIds);
  }
}
