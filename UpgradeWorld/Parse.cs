using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace UpgradeWorld;
public class Parse {
  public static int Int(string arg, int defaultValue = 0) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : defaultValue;
  public static Vector2 Coords(string arg) {
    var values = Split(arg).ToArray();
    Vector2 coords = new();
    if (values.Length > 0) coords.x = Float(values[0]);
    if (values.Length > 1) coords.y = Float(values[1]);
    return coords;
  }

  public static bool TryFloat(string arg, out float number) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
  public static bool IsFloat(string arg) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
  public static float Float(string arg) => float.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture);
  public static IEnumerable<string> Split(string value) => value.Split(',').Select(arg => arg.Trim()).Where(arg => arg != "");
  public static IEnumerable<string> Args(IEnumerable<string> args, int skip) => Split(string.Join(",", args.Skip(skip)));

  public static IEnumerable<string> FiltererArgs(IEnumerable<string> args, FiltererParameters parameters) {
    var parsed = Args(args, 1);
    /*
    List<string> unused = new();
    foreach (var par in parsed) {
      var split = par.Split('=');
      var name = split[0];
      if (name == "zones") parameters.MeasureWithZones = true;
      else if (name == "noedges") parameters.NoEdges = true;
      else if (name == "force") parameters.ForceStart = true;
      else if (split.Length > 1) {
        var value = split[1];
        if (name == "safezones") parameters.SafeZones = Int(value, 2);
        else if (name == "x") parameters.X = Int(value, 0);
        else if (name == "y") parameters.Y = Int(value, 0);
        else if (name == "coords") {

        } else unused.Add(par);
      } else unused.Add(par);
    }
    */
    parsed = NamedInt(parsed, "safezones", ref parameters.SafeZones);
    parsed = Flag(parsed, "zones", out parameters.MeasureWithZones);
    parsed = Flag(parsed, "noedges", out parameters.NoEdges);
    parsed = Flag(parsed, "force", out parameters.ForceStart);
    var other = parsed.Where(arg => !TryFloat(arg, out var _));
    var allNumbers = parsed.Where(arg => TryFloat(arg, out var _)).Select(Float);
    var ranges = other.Where(arg => arg.Split('-').Length == 2 && arg.Split('-').All(IsFloat));
    other = other.Where(arg => arg.Split('-').Length != 2 || !arg.Split('-').All(IsFloat));
    var range = ranges.FirstOrDefault();
    // Add back unused ranges.
    other.ToList().AddRange(ranges.Skip(1));
    if (range != null) {
      var split = range.Split('-').Select(Float);
      parameters.MinDistance = split.First();
      parameters.MaxDistance = split.Last();
    }
    var numbers = allNumbers.Take(range == null ? 3 : 2);
    // Add back unused numbers.
    other.ToList().AddRange(allNumbers.Skip(numbers.Count()).Select(arg => arg.ToString(CultureInfo.InvariantCulture)));
    if (numbers.Count() == 0 && parameters.MeasureWithZones) {
      var zone = ZoneSystem.instance.GetZone(Helper.GetLocalPosition());
      parameters.X = zone.x;
      parameters.Y = zone.y;
    }
    if (numbers.Count() == 1 || numbers.Count() == 3) {
      var distance = numbers.First();
      parameters.MinDistance = 0;
      parameters.MaxDistance = 0;
      if (distance > 0)
        parameters.MinDistance = distance;
      else
        parameters.MaxDistance = -distance;
      if (numbers.Count() == 3) {
        parameters.X = numbers.Skip(1).First();
        parameters.Y = numbers.Last();
      } else {
        if (parameters.MeasureWithZones) {
          var zone = ZoneSystem.instance.GetZone(Helper.GetLocalPosition());
          parameters.X = zone.x;
          parameters.Y = zone.y;
        } else {
          parameters.X = Helper.GetLocalPosition().x;
          parameters.Y = Helper.GetLocalPosition().z;

        }
      }
    }
    if (numbers.Count() == 2) {
      if (range == null) {
        parameters.MinDistance = 0;
        parameters.MaxDistance = 0;
      }
      parameters.X = numbers.First();
      parameters.Y = numbers.Last();
    }

    parameters.Biomes = other.Select(Helper.GetBiome).Where(biome => biome != Heightmap.Biome.BiomesMax && biome != Heightmap.Biome.None).ToHashSet();
    other = other.Where(arg => Helper.GetBiome(arg) == Heightmap.Biome.None);
    parameters.TargetZones = TargetZones.Generated;
    return other;
  }

  public static bool IncludedArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float distance) {
    x = 0;
    z = 0;
    distance = 0;
    if (args.Length < 2) {
      args.Context.AddString("Error: Missing coordinate X");
      return false;
    }
    if (args.Length < 3) {
      args.Context.AddString("Error: Missing coordinate Z");
      return false;
    }
    if (!TryFloat(args[1], out x)) {
      args.Context.AddString("Error: Invalid format for X coordinate.");
      return false;
    }
    if (!TryFloat(args[2], out z)) {
      args.Context.AddString("Error: Invalid format for Z coordinate.");
      return false;
    }
    if (args.Length > 3) {
      if (!TryFloat(args[3], out distance)) {
        args.Context.AddString("Error: Invalid format for distance.");
        return false;
      }
    }
    return true;
  }
  public static IEnumerable<string> Flag(IEnumerable<string> parameters, string flag, out bool value) {
    value = parameters.FirstOrDefault(arg => arg.ToLower() == flag) != null;
    return parameters.Where(arg => arg.ToLower() != flag);
  }
  public static IEnumerable<string> NamedInt(IEnumerable<string> parameters, string key, ref int value) {
    var arg = parameters.FirstOrDefault(arg => arg.ToLower().StartsWith(key + "="));
    if (arg != null && arg != "") {
      var split = arg.Split('=');
      if (split.Length > 0) value = Int(split[1], value);
    }
    return parameters.Where(arg => !arg.ToLower().StartsWith(key + "="));
  }
}