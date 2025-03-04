using System.Linq;

namespace UpgradeWorld;
public class LocationsSwapCommand
{
  public LocationsSwapCommand()
  {
    LocationOperation.Register("locations_swap");
    Helper.Command("locations_swap", "[new id,id1,id2,...] [...args] - Swaps locations to a different one.", (args) =>
    {
      DataParameters pars = new(args, true, false);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new SwapLocations(args.Context, pars.Ids(), pars);
    }, LocationOperation.AllIds);
  }
}
