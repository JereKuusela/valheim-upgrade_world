using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationRegisterCommand
{
  public LocationRegisterCommand()
  {
    CommandWrapper.Register("location_register", (int index, int subIndex) =>
    {
      if (index == 0) return LocationOperation.AllIds();
      if (index == 1) return CommandWrapper.XZY("pos", "Coordinates for the position. If not given, player's position is used", subIndex);
      return null;
    });
    Helper.Command("location_register", "[id] [x,z,y=player position] - Registers a location without placing it.", (args) =>
    {
      if (args.Length == 0)
      {
        Helper.Print(args.Context, "Error: Missing the location id.");
        return;
      }

      if (Helper.IsClient(args)) return;
      var id = args[1];
      var pos = Helper.GetPlayerPosition();
      if (args.Length > 2) pos = Parse.VectorXZY(Parse.Split(args[2]));
      new RegisterLocation(args.Context, id, pos);
    }, LocationOperation.AllIds);
  }
}
