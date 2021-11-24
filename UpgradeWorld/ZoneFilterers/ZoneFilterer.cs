using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  public interface ZoneFilterer {
    Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
  }
  public class FiltererParameters {
    public HashSet<Heightmap.Biome> Biomes = new HashSet<Heightmap.Biome>();
    public bool NoEdges = false;
    public bool ForceStart = false;
    public float X = 0;
    public float Y = 0;
    public float MinDistance = 0;
    public float MaxDistance = 0;
    public bool MeasureWithZones = false;
    public bool NoPlayerBase = false;
    public TargetZones TargetZones = TargetZones.Generated;
    public bool IsBiomeValid(Heightmap.Biome biome) => Biomes.Count() == 0 || Biomes.Contains(biome);
    public bool IsBiomeValid(Vector3 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos));
    public bool IsBiomeValid(Vector2 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos.x, pos.y));

    public IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos) {
      if (Biomes.Count() > 0)
        zdos = zdos.Where(zdo => IsBiomeValid(zdo.GetPosition()));
      if (NoEdges)
        zdos = zdos.Where(zdo => WorldGenerator.instance.GetBiomeArea(zdo.GetPosition()) == Heightmap.BiomeArea.Median);
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

    public override string ToString() {
      var texts = new string[]{
        "Position: " + X + " " + Y,
        "Distance: " + MinDistance + " " + MaxDistance,
        "Biomes: " + string.Join(", ", Biomes)
      };
      return string.Join("\n", texts);
    }
  }
  public static class FiltererFactory {
    public static IEnumerable<ZoneFilterer> Create(FiltererParameters args) {
      var filters = new List<ZoneFilterer>();
      filters.Add(new TargetZonesFilterer(args.TargetZones));
      filters.Add(new BiomeFilterer(args.Biomes, !args.NoEdges));
      filters.Add(new ConfigFilterer());
      if (!args.NoPlayerBase) filters.Add(new PlayerBaseFilterer());
      if (args.MeasureWithZones) filters.Add(new ZoneDistanceFilterer(new Vector2i((int)args.X, (int)args.Y), (int)args.MinDistance, (int)args.MaxDistance));
      else filters.Add(new DistanceFilterer(new Vector3(args.X, 0, args.Y), args.MinDistance, args.MaxDistance));
      return filters;
    }
  }
}