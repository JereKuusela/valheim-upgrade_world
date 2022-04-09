using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public class FiltererParameters {
  public HashSet<Heightmap.Biome> Biomes = new();
  public bool NoEdges = false;
  public bool ForceStart = false;
  public Vector2? Pos = null;
  public Vector2Int? Zone = null;
  public float X = 0;
  public float Y = 0;
  public float MinDistance = 0;
  public float MaxDistance = 0;
  public bool MeasureWithZones = false;
  public int SafeZones = Settings.SafeZoneSize;
  public TargetZones TargetZones = TargetZones.Generated;
  public List<string> Unhandled = new();
  public bool IsBiomeValid(Heightmap.Biome biome) => Biomes.Count() == 0 || Biomes.Contains(biome);
  public bool IsBiomeValid(Vector3 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos));
  public bool IsBiomeValid(Vector2 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos.x, pos.y));

  public FiltererParameters(Terminal.ConsoleEventArgs args) {
    foreach (var par in args.Args.Skip(1).ToArray()) {
      var split = par.Split('=');
      var name = split[0];
      if (name == "noedges") NoEdges = true;
      else if (name == "force") ForceStart = true;
      else if (split.Length > 1) {
        var value = split[1];
        if (name == "safezones") SafeZones = Parse.Int(value, 2);
        else if (name == "pos") Pos = Parse.Pos(value);
        else if (name == "zone") Zone = Parse.Zone(value);
        else if (name == "min" | name == "mindistance") MinDistance = Parse.Float(value);
        else if (name == "max" | name == "maxdistance") MaxDistance = Parse.Float(value);
        else if (name == "distance") {
          var distance = Parse.Pos(value);
          MinDistance = distance.x;
          MaxDistance = distance.y;
        } else if (name == "biomes") Biomes = Parse.Biomes(value);
        else Unhandled.Add(par);
      } else Unhandled.Add(par);
    }
    if (Player.m_localPlayer && !Zone.HasValue && !Pos.HasValue) {
      var pos = Player.m_localPlayer.transform.position;
      Pos = new Vector2(pos.x, pos.z);
    }
  }

  public virtual bool Valid(Terminal terminal) {
    if (Unhandled.Count() > 0) {
      terminal.AddString("Error: Unhandled parameters " + string.Join(", ", Unhandled));
      return false;
    }
    if (Zone.HasValue && Pos.HasValue) {
      terminal.AddString("Error: <color=yellow>pos</color> and <color=yellow>zone</color> can't be used at the same time.");
      return false;
    }
    if (Biomes.Contains(Heightmap.Biome.None)) {
      terminal.AddString("Error: Invalid biomes.");
      return false;
    }
    if (!Zone.HasValue && !Pos.HasValue) {
      terminal.AddString("Error: Position or zone is not defined.");
      return false;
    }
    return true;
  }

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