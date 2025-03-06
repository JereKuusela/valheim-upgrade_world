namespace UpgradeWorld;

public class VegetationResetCommand
{
  public VegetationResetCommand()
  {
    VegetationOperation.Register("vegetation_reset");
    Helper.Command("vegetation_reset", "[id1,id2,...] [...args] - Removes and adds vegetation to generated areas.", (args) =>
    {
      IdParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new ResetVegetation(args.Context, pars.VegIds(), pars));
    }, VegetationOperation.AllIds);
  }
}
