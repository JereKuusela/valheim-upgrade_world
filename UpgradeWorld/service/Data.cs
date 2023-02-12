using UnityEngine;
using UpgradeWorld;

namespace Service;

public class DataHelper
{
  public static string GetData(ZDO zdo, string key, string type)
  {
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    var hashValue = (key + "_i").GetStableHashCode();
    if ((type == "" || type == "vector") && zdo.m_vec3?.ContainsKey(hash) == true)
      return Helper.PrintVectorXZY(zdo.m_vec3[hash]) + " (vector)";
    if ((type == "" || type == "quat") && zdo.m_quats?.ContainsKey(hash) == true)
      return Helper.PrintAngleYXZ(zdo.m_quats[hash]) + " (quat)";
    if ((type == "" || type == "long") && zdo.m_longs?.ContainsKey(hash) == true)
    {
      if (hash == Hash.TimeOfDeath) return Helper.PrintDay(zdo.m_longs[hash]) + " (long)";
      return zdo.m_longs[hash].ToString() + " (long)";
    }
    if ((type == "" || type == "string") && zdo.m_strings?.ContainsKey(hash) == true)
      return zdo.m_strings[hash] + " (string)";
    if ((type == "" || type == "int") && zdo.m_ints?.ContainsKey(hash) == true)
      return zdo.m_ints[hash].ToString() + " (int)";
    if ((type == "" || type == "float") && zdo.m_floats?.ContainsKey(hash) == true)
      return zdo.m_floats[hash].ToString("F1") + " (float)";
    if ((type == "" || type == "id") && zdo.m_longs?.ContainsKey(hashId) == true && zdo.m_longs?.ContainsKey(hashValue) == true)
      return zdo.m_longs[hashId].ToString() + "/" + zdo.m_longs[hashValue].ToString();
    return "No data";
  }

  public static bool SetData(ZDO zdo, string key, string data, string type)
  {
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    if (type == "vector" || (type == "" && zdo.m_vec3?.ContainsKey(hash) == true))
    {
      if (zdo.m_vec3 == null) zdo.m_vec3 = new();
      zdo.m_vec3[hash] = Parse.VectorXZY(Parse.Split(data), Vector3.zero);
    }
    else if (type == "quat" || (type == "" && zdo.m_quats?.ContainsKey(hash) == true))
    {

      if (zdo.m_quats == null) zdo.m_quats = new();
      zdo.m_quats[hash] = Parse.AngleYXZ(data);
    }
    else if (type == "long" || (type == "" && zdo.m_longs?.ContainsKey(hash) == true))
    {
      if (zdo.m_longs == null) zdo.m_longs = new();
      if (hash == Hash.TimeOfDeath) zdo.m_longs[hash] = Helper.ToTick(Parse.Long(data));
      else zdo.m_longs[hash] = Parse.Long(data);
    }
    else if (type == "string" || (type == "" && zdo.m_strings?.ContainsKey(hash) == true))
    {
      if (zdo.m_strings == null) zdo.m_strings = new();
      zdo.m_strings[hash] = data.Replace('_', ' ');
    }
    else if (type == "int" || (type == "" && zdo.m_ints?.ContainsKey(hash) == true))
    {
      if (zdo.m_ints == null) zdo.m_ints = new();
      zdo.m_ints[hash] = Parse.Int(data);
    }
    else if (type == "float" || (type == "" && zdo.m_floats?.ContainsKey(hash) == true))
    {
      if (zdo.m_floats == null) zdo.m_floats = new();
      zdo.m_floats[hash] = Parse.Float(data);
    }
    else if (type == "id" || (type == "" && zdo.m_longs?.ContainsKey(hashId) == true))
    {
      if (zdo.m_longs == null) zdo.m_longs = new();
      var split = Parse.Split(data, '/');
      var hashValue = (key + "_i").GetStableHashCode();
      zdo.m_longs[hashId] = Parse.Long(split[0]);
      zdo.m_longs[hashValue] = Parse.Long(split, 1);
    }
    else
      return false;
    return true;
  }
  public static bool HasData(ZDO zdo, string key, string data, bool includeEmpty)
  {
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    var hashValue = (key + "_i").GetStableHashCode();
    if (zdo.m_vec3?.ContainsKey(hash) == true)
      return Parse.VectorXZYRange(data, Vector3.zero).Includes(zdo.m_vec3[hash]);
    if (zdo.m_quats?.ContainsKey(hash) == true)
      return Parse.AngleYXZ(data) == zdo.m_quats[hash];
    if (zdo.m_longs?.ContainsKey(hash) == true)
    {
      if (hash == Hash.TimeOfDeath) return Parse.LongRange(data).Includes(Helper.ToDay(zdo.m_longs[hash]));
      return Parse.LongRange(data).Includes(zdo.m_longs[hash]);
    }
    if (zdo.m_strings?.ContainsKey(hash) == true)
      return zdo.m_strings[hash] == data.Replace('_', ' '); ;
    if (zdo.m_ints?.ContainsKey(hash) == true)
      return Parse.IntRange(data).Includes(zdo.m_ints[hash]);
    if (zdo.m_floats?.ContainsKey(hash) == true)
      return Parse.FloatRange(data).Includes(zdo.m_floats[hash]);
    if (zdo.m_longs?.ContainsKey(hashId) == true && zdo.m_longs?.ContainsKey(hashValue) == true)
      return data == zdo.m_longs[hashId] + "/" + zdo.m_longs[hashValue];
    return includeEmpty;
  }
}