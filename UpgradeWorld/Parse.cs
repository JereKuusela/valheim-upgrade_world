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
}
