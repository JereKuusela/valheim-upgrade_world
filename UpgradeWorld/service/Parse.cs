using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UpgradeWorld;

namespace Service;
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

///<summary>Contains functions for parsing arguments, etc.</summary>
public static class Parse {
  private static Range<string> Range(string arg) {
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
  public static int Int(string arg, int defaultValue = 0) {
    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static int Int(string[] args, int index, int defaultValue = 0) {
    if (args.Length <= index) return defaultValue;
    return Int(args[index], defaultValue);
  }
  public static Range<int> IntRange(string arg, int defaultValue = 0) {
    var range = Range(arg);
    return new(Int(range.Min, defaultValue), Int(range.Max, defaultValue));
  }
  public static Range<int> IntRange(string[] args, int index, int defaultValue = 0) {
    if (args.Length <= index) return new(defaultValue);
    return IntRange(args[index], defaultValue);
  }
  public static uint UInt(string arg, uint defaultValue = 0) {
    if (!uint.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static uint UInt(string[] args, int index, uint defaultValue = 0) {
    if (args.Length <= index) return defaultValue;
    return UInt(args[index], defaultValue);
  }
  public static Range<uint> UIntRange(string arg, uint defaultValue = 0) {
    var range = Range(arg);
    return new(UInt(range.Min, defaultValue), UInt(range.Max, defaultValue));
  }
  public static Range<uint> UIntRange(string[] args, int index, uint defaultValue = 0) {
    if (args.Length <= index) return new(defaultValue);
    return UIntRange(args[index], defaultValue);
  }
  public static long Long(string arg, long defaultValue = 0) {
    if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static long Long(string[] args, int index, long defaultValue = 0) {
    if (args.Length <= index) return defaultValue;
    return Long(args[index], defaultValue);
  }
  public static Range<long> LongRange(string arg, long defaultValue = 0) {
    var range = Range(arg);
    return new(Long(range.Min, defaultValue), Long(range.Max, defaultValue));
  }
  public static Range<long> LongRange(string[] args, int index, long defaultValue = 0) {
    if (args.Length <= index) return new(defaultValue);
    return LongRange(args[index], defaultValue);
  }
  public static bool TryFloat(string arg, out float value) {
    return float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
  }
  public static float Float(string arg, float defaultValue = 0f) {
    if (!TryFloat(arg, out var result))
      return defaultValue;
    return result;
  }
  public static float Float(string[] args, int index, float defaultValue = 0f) {
    if (args.Length <= index) return defaultValue;
    return Float(args[index], defaultValue);
  }
  public static Range<float> FloatRange(string arg, float defaultValue = 0f) {
    var range = Range(arg);
    return new(Float(range.Min, defaultValue), Float(range.Max, defaultValue));
  }
  public static Range<float> FloatRange(string[] args, int index, float defaultValue = 0f) {
    if (args.Length <= index) return new(defaultValue);
    return FloatRange(args[index], defaultValue);
  }
  public static string String(string[] args, int index, string defaultValue = "") {
    if (args.Length <= index) return defaultValue;
    return args[index];
  }
  public static Quaternion AngleYXZ(string arg) => AngleYXZ(arg, Quaternion.identity);
  public static Quaternion AngleYXZ(string arg, Quaternion defaultValue) {
    var values = Split(arg);
    var angle = Vector3.zero;
    angle.y = Parse.Float(values, 0, defaultValue.eulerAngles.y);
    angle.x = Parse.Float(values, 1, defaultValue.eulerAngles.x);
    angle.z = Parse.Float(values, 2, defaultValue.eulerAngles.z);
    return Quaternion.Euler(angle);
  }
  public static Range<Quaternion> AngleYXZRange(string arg) => AngleYXZRange(arg, Quaternion.identity);
  public static Range<Quaternion> AngleYXZRange(string arg, Quaternion defaultValue) {
    var parts = Split(arg);
    var y = Parse.FloatRange(parts, 0, defaultValue.y);
    var x = Parse.FloatRange(parts, 1, defaultValue.x);
    var z = Parse.FloatRange(parts, 2, defaultValue.z);
    return ToAngleRange(x, y, z);
  }
  private static Range<Quaternion> ToAngleRange(Range<float> x, Range<float> y, Range<float> z) {
    var min = Quaternion.Euler(new(x.Min, y.Min, z.Min));
    var max = Quaternion.Euler(new(x.Max, y.Max, z.Max));
    return new(min, max);
  }
  ///<summary>Parses XZY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args) => VectorXZY(args, 0, Vector3.zero);
  ///<summary>Parses XZY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, Vector3 defaultValue) => VectorXZY(args, 0, defaultValue);
  ///<summary>Parses XZY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index) => VectorXZY(args, index, Vector3.zero);
  ///<summary>Parses XZY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index, Vector3 defaultValue) {
    var vector = Vector3.zero;
    vector.x = Float(args, index, defaultValue.x);
    vector.y = Float(args, index + 2, defaultValue.y);
    vector.z = Float(args, index + 1, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> VectorXZYRange(string arg, Vector3 defaultValue) {
    var parts = Split(arg);
    var x = FloatRange(parts, 0, defaultValue.x);
    var y = FloatRange(parts, 2, defaultValue.y);
    var z = FloatRange(parts, 1, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  ///<summary>Parses ZXY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args) => VectorZXY(args, 0, Vector3.zero);
  ///<summary>Parses ZXY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, Vector3 defaultValue) => VectorZXY(args, 0, defaultValue);
  ///<summary>Parses ZXY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, int index) => VectorZXY(args, index, Vector3.zero);
  ///<summary>Parses ZXY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, int index, Vector3 defaultValue) {
    var vector = Vector3.zero;
    vector.x = Float(args, index + 1, defaultValue.x);
    vector.y = Float(args, index + 2, defaultValue.y);
    vector.z = Float(args, index, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> VectorZXYRange(string arg, Vector3 defaultValue) {
    var parts = Split(arg);
    var x = FloatRange(parts, 1, defaultValue.x);
    var y = FloatRange(parts, 2, defaultValue.y);
    var z = FloatRange(parts, 0, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  private static Range<Vector3> ToVectorRange(Range<float> x, Range<float> y, Range<float> z) {
    Vector3 min = new(x.Min, y.Min, z.Min);
    Vector3 max = new(x.Max, y.Max, z.Max);
    return new(min, max);
  }
  ///<summary>Parses YXZ vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args) => VectorYXZ(args, 0, Vector3.zero);
  ///<summary>Parses YXZ vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, Vector3 defaultValue) => VectorYXZ(args, 0, defaultValue);
  ///<summary>Parses YXZ vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, int index) => VectorYXZ(args, index, Vector3.zero);
  ///<summary>Parses YXZ vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, int index, Vector3 defaultValue) {
    var vector = Vector3.zero;
    vector.y = Float(args, index, defaultValue.y);
    vector.x = Float(args, index + 1, defaultValue.x);
    vector.z = Float(args, index + 2, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> VectorYXZRange(string arg, Vector3 defaultValue) {
    var parts = Split(arg);
    var x = FloatRange(parts, 1, defaultValue.x);
    var y = FloatRange(parts, 0, defaultValue.y);
    var z = FloatRange(parts, 2, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  ///<summary>Parses scale starting at zero index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args) => Scale(args, 0);
  ///<summary>Parses scale starting at given index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args, int index) => SanityCheck(VectorXZY(args, index));
  private static Vector3 SanityCheck(Vector3 scale) {
    // Sanity check and also adds support for setting all values with a single number.
    if (scale.x == 0) scale.x = 1;
    if (scale.y == 0) scale.y = scale.x;
    if (scale.z == 0) scale.z = scale.x;
    return scale;
  }
  public static Range<Vector3> ScaleRange(string arg) {
    var parts = Split(arg);
    var x = FloatRange(parts, 0, 0f);
    var y = FloatRange(parts, 1, 0f);
    var z = FloatRange(parts, 2, 0f);
    var range = ToVectorRange(x, y, z);
    range.Min = SanityCheck(range.Min);
    range.Max = SanityCheck(range.Max);
    return range;
  }

  public static string[] Split(string arg, char separator = ',') => arg.Split(separator).Select(s => s.Trim()).Where(s => s != "").ToArray();
  public static string[] Split(string[] args, int index, char separator) {
    if (args.Length <= index) return new string[0];
    return Split(args[index], separator);
  }
  private static HashSet<string> Truthies = new() {
    "1",
    "true",
    "yes",
    "on"
  };
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static HashSet<string> Falsies = new() {
    "0",
    "false",
    "no",
    "off"
  };
  private static bool IsFalsy(string value) => Falsies.Contains(value);
  public static bool? Boolean(string arg) {
    if (IsTruthy(arg)) return true;
    if (IsFalsy(arg)) return false;
    return null;
  }
  public static IEnumerable<string> Flag(IEnumerable<string> parameters, string flag, out bool value) {
    value = parameters.FirstOrDefault(arg => arg.ToLower() == flag) != null;
    return parameters.Where(arg => arg.ToLower() != flag);
  }
  public static bool Flag(Dictionary<string, string> parameters, string flag) {
    var key = parameters.Keys.FirstOrDefault(key => key.ToLower() == flag.ToLower());
    if (key == null) return false;
    return parameters.Remove(key);
  }

  public static Vector2i Zone(string arg) {
    var values = Split(arg).ToArray();
    Vector2i vector = new();
    if (values.Length > 0) vector.x = Int(values[0]);
    if (values.Length > 1) vector.y = Int(values[1]);
    return vector;
  }
  public static Vector2 Pos(string arg) {
    var values = Split(arg).ToArray();
    Vector2 vector = new();
    if (values.Length > 0) vector.x = Float(values[0]);
    if (values.Length > 1) vector.y = Float(values[1]);
    return vector;
  }
  public static HashSet<Heightmap.Biome> Biomes(string arg) => Split(arg).Select(Helper.GetBiome).ToHashSet();
}
