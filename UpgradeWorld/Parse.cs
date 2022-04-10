using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace UpgradeWorld;
public class Parse {
  public static int Int(string arg, int defaultValue = 0) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : defaultValue;
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
