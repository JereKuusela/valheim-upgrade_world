using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public interface ZoneFilterer {
  Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
}
public class FiltererParameters {
  public HashSet<Heightmap.Biome> Biomes = new();
  public bool NoEdges = false;
  public bool ForceStart = false;
  public float X = 0;
  public float Y = 0;
  public float MinDistance = 0;
  public float MaxDistance = 0;
  public bool MeasureWithZones = false;
  public int SafeZones = Settings.SafeZoneSize;
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
      Vector2i zone = new((int)X, (int)Y);
      var min = (int)MinDistance;
      var max = (int)MaxDistance;
      zdos = zdos.Where(zdo => Zones.IsWithin(zone, ZoneSystem.instance.GetZone(zdo.GetPosition()), min, max));
    } else {
      Vector3 position = new(X, 0, Y);
      if (MinDistance > 0)
        zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) >= MinDistance);
      if (MaxDistance > 0)
        zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= MaxDistance);
    }
    return zdos;
  }

  public override string ToString() {
    var texts = new[]{
        "Position: " + X + " " + Y,
        "Distance: " + MinDistance + " " + MaxDistance,
        "Biomes: " + string.Join(", ", Biomes)
      };
    return string.Join("\n", texts);
  }
  public string Print(string operation) {
    var str = operation;
    if (MinDistance > 0 || MaxDistance > 0) {
      str += " zones";
      if (MinDistance > 0) str += " more than " + MinDistance;
      if (MinDistance > 0 && MaxDistance > 0) str += " and ";
      if (MaxDistance > 0) str += " less than " + MaxDistance;
      if (MeasureWithZones)
        str += " zones";
      else
        str += " meters";
      str += " away from the ";
      if (X == 0 && Y == 0)
        str += "world center";
      else if (X == Helper.GetLocalPosition().x && Y == Helper.GetLocalPosition().z)
        str += "player";
      else if (MeasureWithZones)
        str += "zone " + X + "," + Y;
      else
        str += "coordinates " + X + "," + Y;
    } else if (MeasureWithZones)
      str += " the zone " + X + "," + Y;
    else
      str += " all zones";
    if (Biomes.Count > 0) {
      if (NoEdges)
        str += " that only have " + string.Join(", ", Biomes);
      else
        str += " that have " + string.Join(", ", Biomes);
    }
    var size = 1 + (SafeZones - 1) * 2;
    if (SafeZones <= 0)
      str += " without player base detection";
    else
      str += " with player base detection (" + size + "x" + size + " safe zones)";
    return str;
  }
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
