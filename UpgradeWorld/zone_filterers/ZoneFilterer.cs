using System.Collections.Generic;
namespace UpgradeWorld;
public interface ZoneFilterer {
  Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
}

public static class FiltererFactory {
  public static List<ZoneFilterer> Create(FiltererParameters args) {
    List<ZoneFilterer> filters = new();
    filters.Add(new BiomeFilterer(args.Biomes, !args.NoEdges));
    filters.Add(new ConfigFilterer());
    if (args.Zone.HasValue) filters.Add(new ZoneDistanceFilterer(args.Zone.Value, (int)args.MinDistance, (int)args.MaxDistance));
    else if (args.Pos.HasValue) filters.Add(new DistanceFilterer(new(args.Pos.Value.x, 0, args.Pos.Value.y), args.MinDistance, args.MaxDistance));
    if (args.SafeZones > 0) filters.Add(new PlayerBaseFilterer(args.SafeZones));
    return filters;
  }
}
