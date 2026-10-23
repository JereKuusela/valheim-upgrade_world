namespace UpgradeWorld;

public class PruneSaveSystemCommand
{
  public PruneSaveSystemCommand()
  {
    CommandWrapper.Register("prune_savesystem", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("prune_savesystem", "[...args] - Removes objects and persistent events from non-generated zones (fix for old save system issues).", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new QueuedOperation(args.Context, pars.Pin, "Prune save system.", () =>
      {
        var zdos = EntityOperation.GetZDOs(pars);
        new PruneSaveSystem(args.Context, zdos, pars);
      }), pars.Start);
    });
  }
}
