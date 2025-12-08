using System.Collections.Generic;
namespace UpgradeWorld;

public interface IZoneFilterer
{
  Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
}

public static class FiltererFactory
{
  public static List<IZoneFilterer> Create(FiltererParameters args)
  {
    List<IZoneFilterer> filters = [];
    if (args.Biomes.Count > 0) filters.Add(new BiomeFilterer(args.Biomes, !args.NoEdges));
    if (args.Zone.HasValue && (args.MinDistance != 0f || args.MaxDistance != 0f)) filters.Add(new ZoneDistanceFilterer(args.Zone.Value, (int)args.MinDistance, (int)args.MaxDistance));
    else if (args.Pos.HasValue && (args.MinDistance != 0f || args.MaxDistance != 0f)) filters.Add(new DistanceFilterer(new(args.Pos.Value.x, 0, args.Pos.Value.y), args.MinDistance, args.MaxDistance));
    if (args.SafeZones > 0) filters.Add(new PlayerBaseFilterer(args.SafeZones));
    if (args.Chance < 1f) filters.Add(new ChanceFilterer(args.Chance));
    if (args.LocationIds.Count > 0) filters.Add(new LocationFilterer(args.LocationIds, false));
    return filters;
  }
}
