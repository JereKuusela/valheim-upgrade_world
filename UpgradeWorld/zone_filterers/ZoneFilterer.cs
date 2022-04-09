using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public interface ZoneFilterer {
  Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
}

public static class FiltererFactory {
  public static IEnumerable<ZoneFilterer> Create(FiltererParameters args) {
    List<ZoneFilterer> filters = new();
    filters.Add(new TargetZonesFilterer(args.TargetZones));
    filters.Add(new BiomeFilterer(args.Biomes, !args.NoEdges));
    filters.Add(new ConfigFilterer());
    if (args.MeasureWithZones) filters.Add(new ZoneDistanceFilterer(new((int)args.X, (int)args.Y), (int)args.MinDistance, (int)args.MaxDistance));
    else filters.Add(new DistanceFilterer(new(args.X, 0, args.Y), args.MinDistance, args.MaxDistance));
    if (args.SafeZones > 0) filters.Add(new PlayerBaseFilterer(args.SafeZones));
    return filters;
  }
}
