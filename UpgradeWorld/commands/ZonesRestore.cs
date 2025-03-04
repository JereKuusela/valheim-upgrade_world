namespace UpgradeWorld;
public class ZonesRestoreCommand
{
  public ZonesRestoreCommand()
  {
    CommandWrapper.Register("zones_restore", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("zones_restore", "[...args] - Restores missing zone control objects.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new RestoreZones(args.Context, pars));
    });
  }
}
