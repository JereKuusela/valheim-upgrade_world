using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public static class Commands {
  private static void Mapping() {
    new Terminal.ConsoleCommand("reveal_position", "[x] [y] [distance=0] - Explores the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new ExploreMap(position, distance, true, args.Context);
    }, isCheat: true);
    new Terminal.ConsoleCommand("hide_position", "[x] [y] [distance=0] - Hides the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new ExploreMap(position, distance, false, args.Context);
    }, isCheat: true);
    new Terminal.ConsoleCommand("remove_pins", "[x] [y] [distance=0] - Removes pins from the map at a given position to a given distance.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.ParseIncludedArgs(args, out var x, out var z, out var distance)) return;
      Vector3 position = new(x, 0, z);
      new RemovePins(position, distance, args.Context);
    }, isCheat: true);

  }
  private static bool CheckUnhandled(Terminal.ConsoleEventArgs args, IEnumerable<string> extra, int handled = 0) {
    if (extra.Count() > handled) {
      args.Context.AddString("Error: Unhandled parameters " + string.Join(", ", extra.Skip(handled)));
      return false;
    }
    return true;
  }
  public static bool IsServer(Terminal terminal) {
    if (!ZNet.instance || !ZNet.instance.IsServer()) {
      terminal.AddString("Error: Must be server to use this command.");
      return false;
    }
    return true;
  }
  public static void Init() {
    new Terminal.ConsoleCommand("destroy", "[...args] - Destroys zones which allows the world generator to regenerate them.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var extra = Helper.ParseFiltererArgs(args.Args, parameters);
      if (CheckUnhandled(args, extra))
        Executor.AddOperation(new Destroy(args.Context, parameters));
    }, onlyServer: true, optionsFetcher: () => Helper.AvailableBiomes);

    new Terminal.ConsoleCommand("generate", "[...args] - Generates zones which allows pregenerating the world without having to move there physically.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var extra = Helper.ParseFiltererArgs(args.Args, parameters);
      parameters.TargetZones = TargetZones.Ungenerated;
      parameters.SafeZones = 0;
      if (CheckUnhandled(args, extra))
        Executor.AddOperation(new Generate(args.Context, parameters));
    }, onlyServer: true, optionsFetcher: () => Helper.AvailableBiomes);

    Mapping();

    new Terminal.ConsoleCommand("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var extra = Helper.ParseFiltererArgs(args.Args, parameters);
      var selectedType = "";
      foreach (var type in Upgrade.GetTypes()) {
        extra = Helper.ParseFlag(extra, type, out var found);
        if (found) selectedType = type;
      }
      new Upgrade(args.Context, selectedType, extra, parameters);
    }, onlyServer: true, optionsFetcher: Upgrade.GetTypes);

    new Terminal.ConsoleCommand("place_locations", "[...location_ids] [noclearing] [...args] - Places given location ids to already generated zones.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var extra = Helper.ParseFiltererArgs(args.Args, parameters);
      var ids = Helper.ParseFlag(extra, "noclearing", out var noClearing);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new DistributeLocations(ids, parameters.ForceStart, args.Context));
      Executor.AddOperation(new PlaceLocations(args.Context, !noClearing, parameters));
    }, onlyServer: true, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
    new Terminal.ConsoleCommand("remove_locations", "[...location_ids] [...args] - Removes given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var ids = Helper.ParseFiltererArgs(args.Args, parameters);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new RemoveLocations(args.Context, ids, parameters));
    }, onlyServer: true, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
    new Terminal.ConsoleCommand("regenerate_locations", "[...location_ids] [...args] - Regenerates given location ids.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var ids = Helper.ParseFiltererArgs(args.Args, parameters);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: Missing location ids.");
        return;
      }
      Executor.AddOperation(new RegenerateLocations(args.Context, ids, parameters));
    }, onlyServer: true, optionsFetcher: () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
    new Terminal.ConsoleCommand("reroll_chests", "[chest_name] [looted] [...item_ids] [...args] - Rerolls items at given chests, if they only have given items (all chests if no items specified).", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var extra = Helper.ParseFiltererArgs(args.Args, parameters);
      extra = Helper.ParseFlag(extra, "looted", out var looted);
      if (extra.Count() == 0) {
        args.Context.AddString("Error: Missing chest name.");
        return;
      }
      var chestName = extra.First();
      var ids = extra.Skip(1);
      new RerollChests(chestName, ids, looted, parameters, args.Context);
    }, onlyServer: true, optionsFetcher: () => RerollChests.ChestsNames);
    new Terminal.ConsoleCommand("start", "- Starts execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.DoExecute = true;
    }, onlyServer: true);
    new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.RemoveOperations();
    }, onlyServer: true);
    new Terminal.ConsoleCommand("count_biomes", "[frequency] [...args] - Counts amounts of biomes with given meters of frequency.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
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
      FiltererParameters parameters = new();
      extra = Helper.ParseFiltererArgs(extra, parameters);
      if (CheckUnhandled(args, extra))
        new CountBiomes(args.Context, frequency, parameters);
    }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    new Terminal.ConsoleCommand("count_entities", "[all] [...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var ids = Helper.ParseFiltererArgs(args.Args, parameters);
      ids = Helper.ParseFlag(ids, "all", out var showAll);
      if (ids.Count() == 0)
        new CountAllEntities(args.Context, showAll, parameters);
      else
        new CountEntities(args.Context, ids, parameters);
    }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    new Terminal.ConsoleCommand("list_entities", "[...ids] [...args] - Counts amounts of given entities. If no ids given then counts all entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var ids = Helper.ParseFiltererArgs(args.Args, parameters);
      new ListEntityPositions(args.Context, ids, parameters);
    }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    new Terminal.ConsoleCommand("remove_entities", "[...ids] [...args] - Removes entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      FiltererParameters parameters = new();
      var ids = Helper.ParseFiltererArgs(args.Args, parameters);
      new RemoveEntities(args.Context, ids, parameters);
    }, onlyServer: true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    new Terminal.ConsoleCommand("distribute", "- Redistributes unplaced locations with the genloc command.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      Executor.AddOperation(new DistributeLocations(new string[0], true, args.Context));
    }, onlyServer: true);
    new Terminal.ConsoleCommand("change_time", "[seconds] - Changes time while updating entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing seconds.");
        return;
      }
      var time = Helper.ParseInt(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for seconds.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new ChangeTime(args.Context, time);
    }, onlyServer: true);
    new Terminal.ConsoleCommand("change_day", "[day] - Changes day while updating entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing day.");
        return;
      }
      var time = Helper.ParseInt(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for day.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new ChangeTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
    }, onlyServer: true);
    new Terminal.ConsoleCommand("set_time", "[seconds] - Changes time while updating entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing seconds.");
        return;
      }
      var time = Helper.ParseInt(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for seconds.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new SetTime(args.Context, time);
    }, onlyServer: true);
    new Terminal.ConsoleCommand("set_day", "[day] - Changes day while updating entities.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      if (args.Args.Count() == 0) {
        args.Context.AddString("Error: Missing day.");
        return;
      }
      var time = Helper.ParseInt(args[1], 0);
      if (time == 0) {
        args.Context.AddString("Error: Invalid format for day.");
        return;
      }
      if (args.Args.Count() > 2) {
        args.Context.AddString("Error: Too many parameters.");
        return;
      }
      new SetTime(args.Context, time * EnvMan.instance.m_dayLengthSec);
    }, onlyServer: true);
    new Terminal.ConsoleCommand("set_vegetation", "[...ids] [disable] - Enables/disables vegetation for the world generator.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      var ids = Helper.ParseArgs(args.Args, 1);
      ids = Helper.ParseFlag(args.Args, "disable", out var disable);
      if (ids.Count() == 0) {
        args.Context.AddString("Error: No entity ids given.");
        return;
      }
      new SetVegetation(args.Context, !disable, ids);
    }, onlyServer: true, optionsFetcher: SetVegetation.GetIds);
    new Terminal.ConsoleCommand("reset_vegetation", "- Resets vegetation generation to the default.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      var extra = Helper.ParseArgs(args.Args, 1);
      if (extra.Count() > 0) {
        args.Context.AddString("Error: No parameters expected.");
        return;
      }
      new ResetVegetation(args.Context);
    }, onlyServer: true);
    new Terminal.ConsoleCommand("verbose", "[off] - Toggles the verbose mode.", (Terminal.ConsoleEventArgs args) => {
      if (!IsServer(args.Context)) return;
      Settings.configVerbose.Value = !Settings.Verbose;
      if (Settings.Verbose)
        args.Context.AddString("Verbose mode enabled.");
      else
        args.Context.AddString("Verbose mode disabled.");
    }, onlyServer: true);
  }
}
