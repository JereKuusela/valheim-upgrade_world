using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public interface ZoneFilterer {
    Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
  }
  public class FiltererParameters {
    public IEnumerable<Heightmap.Biome> Biomes;
    public bool IncludeEdges;
    public float X;
    public float Y;
    public float MinDistance;
    public float MaxDistance;
    public bool MeasureWithZones;
    public bool NoPlayerBase;
    public TargetZones TargetZones;
    public bool IsBiomeValid(Heightmap.Biome biome) => Biomes == null || Biomes.Count() == 0 || Biomes.Contains(biome);
    public bool IsBiomeValid(Vector3 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos));
    public bool IsBiomeValid(Vector2 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos.x, pos.y));

    public IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos) {
      if (Biomes.Count() > 0)
        zdos = zdos.Where(zdo => IsBiomeValid(zdo.GetPosition()));
      if (MeasureWithZones) {
        var zone = new Vector2i((int)X, (int)Y);
        var min = (int)MinDistance;
        var max = (int)MaxDistance;
        zdos = zdos.Where(zdo => Zones.IsWithin(zone, ZoneSystem.instance.GetZone(zdo.GetPosition()), min, max));
      } else {
        var position = new Vector3(X, 0, Y);
        if (MinDistance > 0)
          zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) >= MinDistance);
        if (MaxDistance > 0)
          zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= MaxDistance);
      }
      return zdos;
    }
  }
  public static class FiltererFactory {
    public static IEnumerable<ZoneFilterer> Create(FiltererParameters args) {
      var filters = new List<ZoneFilterer>();
      filters.Add(new TargetZonesFilterer(args.TargetZones));
      filters.Add(new BiomeFilterer(args.Biomes, args.IncludeEdges));
      filters.Add(new ConfigFilterer());
      if (!args.NoPlayerBase) filters.Add(new PlayerBaseFilterer());
      if (args.MeasureWithZones) filters.Add(new ZoneDistanceFilterer(new Vector2i((int)args.X, (int)args.Y), (int)args.MinDistance, (int)args.MaxDistance));
      else filters.Add(new DistanceFilterer(new Vector3(args.X, 0, args.Y), args.MinDistance, args.MaxDistance));
      return filters;
    }
  }
}