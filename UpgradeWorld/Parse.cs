using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace UpgradeWorld;

public class Range<T> {
  public T Min;
  public T Max;
  public Range(T value) {
    Min = value;
    Max = value;
  }
  public Range(T min, T max) {
    Min = min;
    Max = max;
  }
}

public class Parse {
  private static Range<string> TryRange(string arg) {
    var range = arg.Split('-').ToList();
    if (range.Count > 1 && range[0] == "") {
      range[0] = "-" + range[1];
      range.RemoveAt(1);
    }
    if (range.Count > 2 && range[1] == "") {
      range[1] = "-" + range[2];
      range.RemoveAt(2);
    }
    if (range.Count == 1) return new(range[0]);
    else return new(range[0], range[1]);
  }
  public static int Int(string arg, int defaultValue = 0) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : defaultValue;
  public static Range<int> TryIntRange(string arg, int defaultValue = 0) {
    var range = TryRange(arg);
    return new(Int(range.Min, defaultValue), Int(range.Max, defaultValue));
  }
  public static Range<int> TryIntRange(string[] args, int index, int defaultValue = 0) {
    if (args.Length <= index) return new(defaultValue);
    return TryIntRange(args[index], defaultValue);
  }
  public static Vector2 Pos(string arg) {
    var values = Split(arg).ToArray();
    Vector2 vector = new();
    if (values.Length > 0) vector.x = Float(values[0]);
    if (values.Length > 1) vector.y = Float(values[1]);
    return vector;
  }

  public static Vector2i Zone(string arg) {
    var values = Split(arg).ToArray();
    Vector2i vector = new();
    if (values.Length > 0) vector.x = Int(values[0]);
    if (values.Length > 1) vector.y = Int(values[1]);
    return vector;
  }
  public static HashSet<Heightmap.Biome> Biomes(string arg) => Split(arg).Select(Helper.GetBiome).ToHashSet();

  public static bool TryFloat(string arg, out float number) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
  public static bool IsFloat(string arg) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
  public static float Float(string arg) => float.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture);
  public static IEnumerable<string> Split(string value) => value.Split(',').Select(arg => arg.Trim()).Where(arg => arg != "");
  public static IEnumerable<string> Flag(IEnumerable<string> parameters, string flag, out bool value) {
    value = parameters.FirstOrDefault(arg => arg.ToLower() == flag) != null;
    return parameters.Where(arg => arg.ToLower() != flag);
  }
}
