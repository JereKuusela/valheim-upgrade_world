using Service;

namespace UpgradeWorld;

public class LocationUnregisterCommand
{
  public LocationUnregisterCommand()
  {
    CommandWrapper.Register("location_unregister", (index, subIndex) =>
    {
      if (index == 0) return CommandWrapper.XZY("pos", "Coordinates for the position. If not given, player's position is used", subIndex);
      return null;
    });
    Helper.Command("location_unregister", "[x,z,y=player position] - Removes location registration.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      var pos = Helper.GetPlayerPosition();
      if (args.Length > 1) pos = Parse.VectorXZY(Parse.Split(args[1]));
      new UnregisterLocation(args.Context, pos);
    });
  }
}
