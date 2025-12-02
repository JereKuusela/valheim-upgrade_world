using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld;
/// <summary>Predefined upgrade operations</summary>
public class Upgrade : BaseOperation
{
  public static List<string> Types =
  [
    "tarpits",
    "onions",
    "mistlands",
    "mistlands_worldgen",
    "hh_worldgen",
    "legacy_worldgen",
    "mountain_caves",
    "hildir",
    "ashlands",
    "deepnorth",
    "bogwitch",
    "combatruins"
  ];

  public Upgrade(Terminal context, string type, List<string> extra, FiltererParameters args) : base(context)
  {
    AddOperations(type, extra, args);
    Types.Sort();
  }

  private void AddOperations(string type, List<string> extra, FiltererParameters args)
  {
    if (type == null || type == "")
    {
      Print("Error: Missing upgrade type");
      return;
    }
    type = type.ToLower();
    if (type == "ashlands")
    {
      if (args.Biomes.Count() > 0)
      {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Pos = new(0, 4000f);
      args.MinDistance = 11600;
      var ids = ZoneSystem.instance.m_locations.Where(loc => Helper.IsValid(loc) && loc.m_biome == Heightmap.Biome.AshLands && loc.m_enable).Select(loc => loc.m_prefab.Name).ToHashSet();
      Executor.AddOperation(new RemoveLocations(Context, [], args), false);
      Executor.AddOperation(new ResetZones(Context, args), false);
      if (ids.Count() > 0)
        Executor.AddOperation(new DistributeLocations(Context, ids, args), false);
      Executor.AddOperation(new TempleVersion(Context, "ashlands"), args.Start);
    }
    else if (type == "deepnorth")
    {
      if (args.Biomes.Count() > 0)
      {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Pos = new(0, -4000f);
      args.MinDistance = 11600;
      var ids = ZoneSystem.instance.m_locations.Where(loc => Helper.IsValid(loc) && loc.m_biome == Heightmap.Biome.DeepNorth && loc.m_enable).Select(loc => loc.m_prefab.Name).ToHashSet();
      Executor.AddOperation(new RemoveLocations(Context, [], args), false);
      Executor.AddOperation(new ResetZones(Context, args), false);
      if (ids.Count() > 0)
        Executor.AddOperation(new DistributeLocations(Context, ids, args), args.Start);
    }
    else if (type == "mountain_caves")
    {
      Executor.AddOperation(new DistributeLocations(Context, ["MountainCave02"], args), false);
      Executor.AddOperation(new SpawnLocations(Context, ["MountainCave02"], args), args.Start);
    }
    else if (type == "bogwitch")
    {
      Executor.AddOperation(new DistributeLocations(Context, ["BogWitch_Camp"], args), false);
      Executor.AddOperation(new SpawnLocations(Context, ["BogWitch_Camp"], args), args.Start);
    }
    else if (type == "combatruins")
    {
      Executor.AddOperation(new DistributeLocations(Context, ["CombatRuin01"], args), false);
      Executor.AddOperation(new SpawnLocations(Context, ["CombatRuin01"], args), args.Start);
    }
    else if (type == "tarpits")
    {
      Executor.AddOperation(new DistributeLocations(Context, ["TarPit1", "TarPit2", "TarPit3"], args), false);
      Executor.AddOperation(new SpawnLocations(Context, ["TarPit1", "TarPit2", "TarPit3"], args), args.Start);
    }
    else if (type == "onions")
    {
      if (extra.Count() > 0)
      {
        Print("Error: This operation doesn't support extra parameters " + string.Join(", ", extra));
        return;
      }
      new ResetChests(["TreasureChest_mountains"], ["Amber", "Coins", "AmberPearl", "Ruby", "Obsidian", "ArrowFrost", "OnionSeeds"], args.Start, new(args), Context);
    }
    else if (type == "mistlands")
    {
      if (args.Biomes.Count() > 0)
      {
        Print("Error: This operation doesn't support custom biomes " + string.Join(", ", args.Biomes));
        return;
      }
      args.Biomes = [Heightmap.Biome.Mistlands];
      Executor.AddOperation(new ResetZones(Context, args), false);
      var ids = ZoneSystem.instance.m_locations.Where(loc => Helper.IsValid(loc) && loc.m_biome == Heightmap.Biome.Mistlands && loc.m_enable).Select(loc => loc.m_prefab.Name).ToHashSet();
      Executor.AddOperation(new DistributeLocations(Context, ids, args), args.Start);
    }
    else if (type == "mistlands_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 2), false);
      args.MinDistance = 5901;
      Executor.AddOperation(new RemoveLocations(Context, [.. LocationOperation.AllIds()], args), false);
      Executor.AddOperation(new DistributeLocations(Context, [.. LocationOperation.AllIds()], args), false);
      Executor.AddOperation(new ResetZones(Context, args), false);
      Executor.AddOperation(new Print(Context, "If you don't want to automatically reset outer areas, use <color=yellow>stop</color> and then <color=yellow>world_gen mistlands</color> commands.", ""), args.Start);
    }
    else if (type == "hh_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 1), false);
      Executor.AddOperation(new Print(Context, "To reset outer areas, use <color=yellow>world_reset minDistance=5900</color> command.", ""), args.Start);
    }
    else if (type == "legacy_worldgen")
    {
      Executor.AddOperation(new WorldVersion(Context, 0), args.Start);
    }
    else if (type == "hildir")
    {
      Executor.AddOperation(new DistributeLocations(Context, ["Hildir_plainsfortress", "Hildir_crypt", "Hildir_camp", "Hildir_cave"], args), false);
      Executor.AddOperation(new SpawnLocations(Context, ["Hildir_plainsfortress", "Hildir_crypt", "Hildir_camp", "Hildir_cave"], args), args.Start);
    }
    else
      Print("Error: Invalid upgrade type");
  }
}
