using System.Linq;
namespace UpgradeWorld;
public class LocationsResetCommand
{
  public LocationsResetCommand()
  {
    LocationOperation.Register("locations_reset");
    Helper.Command("locations_reset", "[id1,id2,...] [...args] - Resets locations by removing them and then placing them at the same position. Dungeons which have a random rotation will also get a new layout.", (args) =>
    {
      LocationIdParameters pars = new(args);
      if (Helper.IsClient(args)) return;
      if (!pars.Valid(args.Context)) return;
      Executor.AddOperation(new RegenerateLocations(args.Context, pars.Ids(), pars));
    }, LocationOperation.AllIds);
  }
}
