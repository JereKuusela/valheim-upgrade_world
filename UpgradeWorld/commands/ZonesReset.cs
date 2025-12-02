namespace UpgradeWorld;

public class ZonesResetCommand
{
  public ZonesResetCommand()
  {
    CommandWrapper.Register("zones_reset", index =>
    {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    Helper.Command("zones_reset", "[...args] - Destroys areas making them ungenerated. These areas will be generated when visited.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new ResetZones(args.Context, pars), pars.Start);
    }, () => FiltererParameters.Parameters);
  }
}
