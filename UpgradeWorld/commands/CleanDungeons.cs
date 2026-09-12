namespace UpgradeWorld;

public class CleanDungeonsCommand
{
  public CleanDungeonsCommand()
  {
    CommandWrapper.Register("clean_dungeons", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_dungeons", "[...args] - Optimizes old dungeons.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new QueuedOperation(args.Context, pars.Pin, "Clean dungeons.", () =>
      {
        var zdos = EntityOperation.GetZDOs(pars);
        new CleanDungeons(args.Context, zdos, pars.Pin, true);
      }), pars.Start);
    });
  }
}
