using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public static class Commands {
    private static void Mapping() {
      new Terminal.ConsoleCommand("reveal_position", "[x] [y] [distance=0] - Explores the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new ExploreMap(position, distance, true, args.Context);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("hide_position", "[x] [y] [distance=0] - Hides the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new ExploreMap(position, distance, false, args.Context);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("remove_pins", "[x] [y] [distance=0] - Removes pins from the map at a given position to a given distance.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
        var position = new Vector3(x, 0, z);
        new RemovePins(position, distance, args.Context);
      }, onlyServer: true);

    }
    private static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0) {
      if (extra.Count() > handled) {
        args.Context.AddString("Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
        return false;
      }
      return true;
    }
    public static void Init() {
      new Terminal.ConsoleCommand("destroy", "[...args] - Destroys zones which allows the world generator to regenerate them.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = Helper.ParseFiltererArgs(args.Args, parameters);
        if (CheckUnhandled(args, extra))
          Executor.AddOperation(new Destroy(args.Context, parameters));
      }, onlyServer: true, optionsFetcher: () => Helper.AvailableBiomes);

      new Terminal.ConsoleCommand("generate", "[...args] - Generates zones which allows pregenerating the world without having to move there physically.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = Helper.ParseFiltererArgs(args.Args, parameters);
        parameters.TargetZones = TargetZones.Generated;
        parameters.IncludePlayerBases = true;
        if (CheckUnhandled(args, extra))
          Executor.AddOperation(new Generate(args.Context, parameters));
      }, onlyServer: true, optionsFetcher: () => Helper.AvailableBiomes);

      Mapping();

      new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = Helper.ParseFiltererArgs(args.Args, parameters);
        var selectedType = "";
        foreach (var type in Upgrade.GetTypes()) {
          extra = Helper.ParseFlag(extra, type, out var found);
          if (found) selectedType = type;
        }
        new Upgrade(args.Context, selectedType, extra, parameters);
      }, onlyServer: true, optionsFetcher: Upgrade.GetTypes);

      new Terminal.ConsoleCommand("place_locations", "[...location_ids] [noclearing] [...args] - Places given location ids to already generated zones.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = Helper.ParseFiltererArgs(args.Args, parameters);
        var ids = Helper.ParseFlag(extra, "noclearing", out var noClearing);
        if (ids.Count() == 0) {
          args.Context.AddString("Error: Missing location ids.");
          return;
        }
        // TODO: Should validate location ids / provide parameter list.
        Executor.AddOperation(new DistributeLocations(ids, parameters.ForceStart, args.Context));
        Executor.AddOperation(new PlaceLocations(args.Context, !noClearing, parameters));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("reroll_chests", "[chest_name] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var extra = Helper.ParseFiltererArgs(args.Args, parameters);
        if (extra.Count() == 0) {
          args.Context.AddString("Error: Missing chest name.");
          return;
        }
        var chestName = extra.First();
        var ids = extra.Skip(1);
        new RerollChests(chestName, ids, parameters, args.Context);
      }, onlyServer: true, optionsFetcher: () => RerollChests.ChestsNames);
      new Terminal.ConsoleCommand("start", "- Starts execution of operations.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.DoExecute = true;
      }, onlyServer: true);
      new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.RemoveOperations();
      }, onlyServer: true);
      new Terminal.ConsoleCommand("count_biomes", "[frequency] [...args] - Counts amounts of biomes with given meters of frequency.", delegate (Terminal.ConsoleEventArgs args) {
        var frequency = 100f;
        if (args.Args.Count() < 2) {
          args.Context.AddString("Error: Missing frequency.");
          return;
        }
        if (!Helper.TryParseFloat(args.Args[1], out frequency)) {
          args.Context.AddString("Error: Frequency has wrong format.");
          return;
        }
        // Remove only one was ParseFiltererArgs will also remove one.
        var extra = Helper.ParseArgs(args.Args, 1);
        var parameters = new FiltererParameters();
        extra = Helper.ParseFiltererArgs(extra, parameters);
        if (CheckUnhandled(args, extra))
          new CountBiomes(args.Context, frequency, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("count_entities", "[all] [...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = Helper.ParseFiltererArgs(args.Args, parameters);
        ids = Helper.ParseFlag(ids, "all", out var showAll);
        if (ids.Count() == 0)
          new CountAllEntities(args.Context, showAll, parameters);
        else
          new CountEntities(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("list_entities", "[...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = Helper.ParseFiltererArgs(args.Args, parameters);
        new ListEntityPositions(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("remove_entities", "[...ids] [...args] - Removes entities.", delegate (Terminal.ConsoleEventArgs args) {
        var parameters = new FiltererParameters();
        var ids = Helper.ParseFiltererArgs(args.Args, parameters);
        new RemoveEntities(args.Context, ids, parameters);
      }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command.", delegate (Terminal.ConsoleEventArgs args) {
        Executor.AddOperation(new DistributeLocations(new string[0], true, args.Context));
      }, onlyServer: true);
      new Terminal.ConsoleCommand("change_time", "[seconds] - Changes time while updating entities.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Args.Count() == 0) {
          args.Context.AddString("Error: Missing seconds.");
          return;
        }
        if (!Helper.ParseInt(args[1], out var time)) {
          args.Context.AddString("Error: Invalid format for seconds.");
          return;
        }
        if (args.Args.Count() > 2) {
          args.Context.AddString("Error: Too many parameters.");
          return;
        }
        new ChangeTime(args.Context, time);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("change_day", "[day] - Changes day while updating entities.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Args.Count() == 0) {
          args.Context.AddString("Error: Missing day.");
          return;
        }
        if (!Helper.ParseInt(args[1], out var time)) {
          args.Context.AddString("Error: Invalid format for day.");
          return;
        }
        if (args.Args.Count() > 2) {
          args.Context.AddString("Error: Too many parameters.");
          return;
        }
        new ChangeTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("set_vegetation", "[...ids] [disable] - Enables/disables vegetation for the world generator.", delegate (Terminal.ConsoleEventArgs args) {
        var ids = Helper.ParseArgs(args.Args, 1);
        ids = Helper.ParseFlag(args.Args, "disable", out var disable);
        if (ids.Count() == 0) {
          args.Context.AddString("Error: No entity ids given.");
          return;
        }
        new SetVegetation(args.Context, !disable, ids);
      }, onlyServer: true, optionsFetcher: SetVegetation.GetIds);
      new Terminal.ConsoleCommand("reset_vegetation", "- Resets vegetation generation to the default.", delegate (Terminal.ConsoleEventArgs args) {
        var extra = Helper.ParseArgs(args.Args, 1);
        if (extra.Count() > 0) {
          args.Context.AddString("Error: No parameters expected.");
          return;
        }
        new ResetVegetation(args.Context);
      }, onlyServer: true);
      new Terminal.ConsoleCommand("verbose", "[off] - Toggles the verbose mode.", delegate (Terminal.ConsoleEventArgs args) {
        Settings.configVerbose.Value = !Settings.Verbose;
        if (Settings.Verbose)
          args.Context.AddString("Verbose mode enabled.");
        else
          args.Context.AddString("Verbose mode disabled.");
      }, onlyServer: true);
    }
  }
}