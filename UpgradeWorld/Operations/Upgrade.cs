using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Predefined upgrade operations</summary>
public class Upgrade : BaseOperation
{
  public static List<string> Types = new()
  {
    "tarpits",
    "onions",
    "mistlands",
    "mistlands_worldgen",
    "hh_worldgen",
    "legacy_worldgen",
    "mountain_caves",
    "EVA",
    "jewelcrafting"
  };

  public Upgrade(Terminal context, string type, Dictionary<string, string> extra, FiltererParameters args) : base(context)
  {
    AddOperations(type, extra, args);
    Types.Sort();
  }

  private void AddOperations(string type, Dictionary<string, string> extra, FiltererParameters args)
  {
    if (type == null || type == "")
    {
      Print("Error: Missing upgrade type");
      return;
    }
    type = type.ToLower();
    if (type == "mountain_caves")
    {
      var noClearing = Parse.Flag(extra, "noclearing");
      Executor.AddOperation(new DistributeLocations(new[] { "MountainCave02" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    }
    else if (type == "jewelcrafting")
    {
      var noClearing = Parse.Flag(extra, "noclearing");
      Executor.AddOperation(new DistributeLocations(new[] { "JC_Gacha_Location" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      Executor.AddOperation(new Print(Context, "Note: This is for Jewelcrafting mod.", "", args.Start));
    }
    else if (type == "tarpits")
    {
      var noClearing = Parse.Flag(extra, "noclearing");
      Executor.AddOperation(new DistributeLocations(new[] { "TarPit1", "TarPit2", "TarPit3" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
    }
    else if (type == "onions")
    {
      if (extra.Count() > 0)
      {
        Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
        return;
      }
      new ResetChests(new[] { "TreasureChest_mountains" }, new[] { "Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds" }, args.Start, new(args), Context);
    }
    else if (type == "mistlands")
    {
      if (args.Biomes.Count() > 0)
      {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Mistlands };
      Executor.AddOperation(new ResetZones(Context, args));
      Executor.AddOperation(new DistributeLocations(new string[0], args.Start, args.Chance, Context));
    }
    else if (type == "mistlands_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 2, args.Start));
      args.MinDistance = 5901;
      Executor.AddOperation(new RemoveLocations(Context, new string[0], args));
      Executor.AddOperation(new DistributeLocations(new string[0], args.Start, args.Chance, Context));
      Executor.AddOperation(new ResetZones(Context, args));
      Executor.AddOperation(new Print(Context, "If you don't want to automatically reset outer areas, use <color=yellow>stop</color> and then <color=yellow>world_gen mistlands</color> commands.", "", args.Start));
    }
    else if (type == "hh_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 1, args.Start));
      Executor.AddOperation(new Print(Context, "", "To reset outer areas, use <color=yellow>world_reset minDistance=5900</color> command.", args.Start));
    }
    else if (type == "legacy_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 0, args.Start));
    }
    else if (type == "eva")
    {
      if (args.Biomes.Count() > 0)
      {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      var noClearing = Parse.Flag(extra, "noclearing");
      args.Biomes = new HashSet<Heightmap.Biome> { Heightmap.Biome.Plains, Heightmap.Biome.DeepNorth, Heightmap.Biome.Mistlands, Heightmap.Biome.AshLands };
      var safeZones = args.SafeZones;
      args.SafeZones = 0;
      Executor.AddOperation(new RemoveLocations(Context, new[] { "SvartalfrQueenAltar" }, args));
      args.SafeZones = safeZones;
      Executor.AddOperation(new DistributeLocations(new[] { "BlazingDamnedOneAltar", "JotunnAltar", "SvartalfrQueenAltar_New", "Vegvisir_BlazingDamnedOne", "Vegvisir_Jotunn", "Vegvisir_SvartalfrQueen" }, args.Start, args.Chance, Context));
      Executor.AddOperation(new PlaceLocations(Context, !noClearing, args));
      Executor.AddOperation(new RemoveVegetation(Context, new() { "BurningTree", "FrometalVein_frac", "HeavymetalVein" }, args));
      Executor.AddOperation(new AddVegetation(Context, new() { "BurningTree", "FrometalVein_frac", "HeavymetalVein" }, args));
      Executor.AddOperation(new Print(Context, "Note: This is for Epic Valheim Additions mod.", "", args.Start));
    }
    else
      Print("Error: Invalid upgrade type");
  }
}
