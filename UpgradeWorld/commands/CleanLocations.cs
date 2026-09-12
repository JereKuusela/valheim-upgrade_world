namespace UpgradeWorld;

public class CleanLocationsCommand
{
  public CleanLocationsCommand()
  {
    CommandWrapper.Register("clean_locations", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("clean_locations", "[...args] - Removes missing locations.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new QueuedOperation(args.Context, pars.Pin, "Clean locations.", () =>
      {
        var zdos = EntityOperation.GetZDOs(pars);
        new CleanLocations(args.Context, zdos, pars.Pin, true);
      }), pars.Start);
    });
  }
}
