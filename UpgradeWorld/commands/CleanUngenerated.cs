namespace UpgradeWorld;

public class CleanUngeneratedCommand
{
  public CleanUngeneratedCommand()
  {
    CommandWrapper.Register("clean_ungenerated", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_ungenerated", "[...args] - Removes objects from non-generated zones.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new QueuedOperation(args.Context, pars.Pin, "Clean ungenerated.", () =>
      {
        var zdos = EntityOperation.GetZDOs(pars);
        new CleanUngenerated(args.Context, zdos, pars.Pin, true);
      }), pars.Start);
    });
  }
}
