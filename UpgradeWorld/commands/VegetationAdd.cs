namespace UpgradeWorld;

public class VegetationAddCommand
{
  public VegetationAddCommand()
  {
    VegetationOperation.Register("vegetation_add");
    Helper.Command("vegetation_add", "[id1,id2,...] [...args] - Adds vegetation to generated areas.", (args) =>
    {
      VegetationIdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new AddVegetation(args.Context, pars.Ids, pars), pars.Start);
    }, VegetationOperation.AllIds);
  }
}
