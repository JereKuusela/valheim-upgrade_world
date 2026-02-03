using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class LocationRegisterCommand
{
  public LocationRegisterCommand()
  {
    Dictionary<string, Func<int, List<string>?>> named = new()
    {
      { "pos", subIndex => CommandWrapper.XZY("pos", "Coordinates for the position. If not given, player's position is used", subIndex) }
    };
    CommandWrapper.Register("location_register", (index, subIndex) =>
      {
        if (index == 0) return LocationOperation.AllIds();
        if (index == 1) return CommandWrapper.XZY("pos", "Coordinates for the position. If not given, player's position is used", subIndex);
        return null;
      }, named);
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
      if (args.Length > 2)
      {
        var pieces = Parse.Split(args[2], '=');
        if (pieces.Length == 1)
        {
          pos = Parse.VectorXZY(Parse.Split(pieces[0]));
        }
        else if (pieces[0].ToLower() == "pos")
        {
          pos = Parse.VectorXZY(Parse.Split(pieces[1]));
        }
        else
        {
          Helper.Print(args.Context, "Error: Invalid position argument.");
          return;
        }
      }
      new RegisterLocation(args.Context, id, pos);
    }, LocationOperation.AllIds);
  }
}
