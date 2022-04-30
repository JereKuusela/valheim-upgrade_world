using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
public class FiltererParameters {
  public HashSet<Heightmap.Biome> Biomes = new();
  public bool NoEdges = false;
  public bool Start = false;
  public Vector2? Pos = null;
  public Vector2i? Zone = null;
  public float MinDistance = 0;
  public float MaxDistance = 0;
  public float Chance = 1f;
  public int SafeZones = Settings.SafeZoneSize;
  public TargetZones TargetZones = TargetZones.Generated;
  public List<string> Unhandled = new();
  public bool IsBiomeValid(Heightmap.Biome biome) => Biomes.Count() == 0 || Biomes.Contains(biome);
  public bool IsBiomeValid(Vector3 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos));
  public bool IsBiomeValid(Vector2 pos) => IsBiomeValid(WorldGenerator.instance.GetBiome(pos.x, pos.y));

  public FiltererParameters(Terminal.ConsoleEventArgs args) {
    foreach (var par in args.Args.Skip(1).ToArray()) {
      var split = par.Split('=');
      var name = split[0].ToLower();
      if (split.Length > 1) {
        var value = split[1];
        if (name == "safezones") SafeZones = Parse.Int(value, 2);
        else if (name == "pos") Pos = Parse.Pos(value);
        else if (name == "zone") Zone = Parse.Zone(value);
        else if (name == "min" | name == "mindistance") MinDistance = Parse.Float(value);
        else if (name == "max" | name == "maxdistance") MaxDistance = Parse.Float(value);
        else if (name == "chance") Chance = Parse.Float(value) / 100f;
        else if (name == "distance") {
          var distance = Parse.Pos(value);
          MinDistance = distance.x;
          MaxDistance = distance.y;
        } else if (name == "biomes") Biomes = Parse.Biomes(value);
        else Unhandled.Add(par);
      } else if (name == "noedges") NoEdges = true;
      else if (name == "start") Start = true;
      else if (name == "zone") Zone = Helper.GetPlayerZone();
      else if (name == "force") SafeZones = 0;
      else Unhandled.Add(par);
    }
    if (!Zone.HasValue && !Pos.HasValue) {
      var pos = Helper.GetPlayerPosition();
      Pos = new Vector2(pos.x, pos.z);
    }
  }

  public virtual bool Valid(Terminal terminal) {
    if (Unhandled.Count() > 0) {
      Helper.Print(terminal, "Error: Unhandled parameters " + string.Join(", ", Unhandled));
      return false;
    }
    if (Zone.HasValue && Pos.HasValue) {
      Helper.Print(terminal, "Error: <color=yellow>pos</color> and <color=yellow>zone</color> can't be used at the same time.");
      return false;
    }
    if (Biomes.Contains(Heightmap.Biome.None)) {
      Helper.Print(terminal, "Error: Invalid biomes.");
      return false;
    }
    if (!Zone.HasValue && !Pos.HasValue) {
      Helper.Print(terminal, "Error: Position or zone is not defined.");
      return false;
    }
    return true;
  }

  public IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos) {
    if (Biomes.Count() > 0)
      zdos = zdos.Where(zdo => IsBiomeValid(zdo.GetPosition()));
    if (NoEdges)
      zdos = zdos.Where(zdo => WorldGenerator.instance.GetBiomeArea(zdo.GetPosition()) == Heightmap.BiomeArea.Median);
    if (Zone.HasValue) {
      var zone = Zone.Value;
      var min = (int)MinDistance;
      var max = (int)MaxDistance;
      zdos = zdos.Where(zdo => Zones.IsWithin(zone, ZoneSystem.instance.GetZone(zdo.GetPosition()), min, max));
    } else if (Pos.HasValue) {
      Vector3 position = new(Pos.Value.x, 0, Pos.Value.y);
      if (MinDistance > 0)
        zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) >= MinDistance);
      if (MaxDistance > 0)
        zdos = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= MaxDistance);
    }
    return zdos;
  }

  public override string ToString() {
    var position = "";
    if (Zone.HasValue) position = $"{Zone.Value.x} {Zone.Value.y}";
    if (Pos.HasValue) position = $"{Pos.Value.x} {Pos.Value.y}";
    var texts = new[]{
        "Position: " + position,
        "Distance: " + MinDistance + " " + MaxDistance,
        "Biomes: " + string.Join(", ", Biomes)
      };
    return string.Join("\n", texts);
  }
  public string Print(string operation) {
    var str = operation + " ";
    if (Biomes.Count > 0) {
      str += string.Join(", ", Biomes);
      if (NoEdges)
        str += " (excluding biome edges)";
    } else
      str += "zones";
    if (MinDistance > 0 || MaxDistance > 0) {
      if (MinDistance > 0) str += " more than " + (MinDistance - 1);
      if (MinDistance > 0 && MaxDistance > 0) str += " and";
      if (MaxDistance > 0) str += " less than " + (MaxDistance + 1);
      if (Zone.HasValue)
        str += " zones";
      else
        str += " meters";
      str += " away from the ";
      if (Zone.HasValue)
        str += "zone " + Zone.Value.x + "," + Zone.Value.y;
      else if (Pos.HasValue && Pos.Value.x == 0 && Pos.Value.y == 0)
        str += "world center";
      else if (Pos.HasValue && Pos.Value.x == Helper.GetPlayerPosition().x && Pos.Value.y == Helper.GetPlayerPosition().z)
        str += "player";
      else if (Pos.HasValue)
        str += "coordinates " + Pos.Value.x + "," + Pos.Value.y;
    } else if (Zone.HasValue)
      str += " at index " + Zone.Value.x + "," + Zone.Value.y;
    var size = 1 + (SafeZones - 1) * 2;
    if (SafeZones <= 0)
      str += ". No player base detection.";
    else
      str += ". Player base detection (" + size + "x" + size + " safe zones).";
    return str;
  }
  public static List<string> Parameters = new() {
    "pos", "zone", "biomes", "min", "minDistance", "max", "maxDistance", "distance", "start", "noEdges", "safeZones", "chance", "force"
  };
  public static Dictionary<string, Func<int, List<string>?>> GetAutoComplete() {
    return new() {
      { "pos", (int index) => CommandWrapper.XZ("pos", "Coordinates for the center point. If not given, player's position is used", index)},
      { "zone", (int index) => CommandWrapper.XZ("zone" , "Indices for the center zone", index) },
      { "biomes", (int index) => Helper.AvailableBiomes },
      { "min", (int index) => index == 0 ? CommandWrapper.Info("min=<color=yellow>meters or zones</color> | Minimum distance from the center point / zone.") : null },
      { "minDistance", (int index) => index == 0 ? CommandWrapper.Info("minDistance=<color=yellow>meters or zones</color> | Minimum distance from the center point / zone.") : null },
      { "max", (int index) => index == 0 ? CommandWrapper.Info("max=<color=yellow>meters or zones</color> | Maximum distance from the center point / zone.") : null },
      { "maxDistance", (int index) => index == 0 ? CommandWrapper.Info("maxDistance=<color=yellow>meters or zones</color> | Maximum distance from the center point / zone.") : null },
      { "safeZones", (int index) => index == 0 ? CommandWrapper.Info("safezones=<color=yellow>amount</color> | The size of protected areas around player base structures.") : null },
      { "chance", (int index) => index == 0 ? CommandWrapper.Info("chance=<color=yellow>percentage</color> (from 0 to 100) | The chance of a single operation being done.") : null },
      { "start", (int index) => CommandWrapper.Flag("start", "Starts the operation instantly") },
      { "force", (int index) => CommandWrapper.Flag("force", "Disables the player base detection") },
      { "noEdges", (int index) => CommandWrapper.Flag("noedges", "Excludes zones with multiple biomes") },
      { "distance", (int index) => {
          if (index == 0) return CommandWrapper.Info("distance=<color=yellow>min</color>,max | Minimum distance from the center point / zone.");
          if (index == 1) return CommandWrapper.Info("distance=min,<color=yellow>max</color> | Maximum distance from the center point / zone.");
          return null;
        }
      }
    };
  }
  public static System.Random random = new();
  public bool Roll() {
    if (Chance >= 1f) return true;
    return random.NextDouble() < Chance;
  }
}
