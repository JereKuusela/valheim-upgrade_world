using UnityEngine;
using UpgradeWorld;

namespace Service;

public class DataHelper {
  public static string GetData(ZDO zdo, string key, string type) {
    var id = zdo.m_uid;
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    var hashValue = (key + "_i").GetStableHashCode();
    var hasVec = ZDOExtraData.s_vec3.ContainsKey(id) && ZDOExtraData.s_vec3[id].ContainsKey(hash);
    var hasQuat = ZDOExtraData.s_quats.ContainsKey(id) && ZDOExtraData.s_quats[id].ContainsKey(hash);
    var hasLong = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hash);
    var hasString = ZDOExtraData.s_strings.ContainsKey(id) && ZDOExtraData.s_strings[id].ContainsKey(hash);
    var hasInt = ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].ContainsKey(hash);
    var hasFloat = ZDOExtraData.s_floats.ContainsKey(id) && ZDOExtraData.s_floats[id].ContainsKey(hash);
    var hasId = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hashId) && ZDOExtraData.s_longs[id].ContainsKey(hashValue);
    if (hasVec && (type == "" || type == "vector")) return Helper.PrintVectorXZY(ZDOExtraData.s_vec3[id][hash]) + " (vector)";
    if (hasQuat && (type == "" || type == "quat")) return Helper.PrintAngleYXZ(ZDOExtraData.s_quats[id][hash]) + " (quat)";
    if (hasLong && (type == "" || type == "long")) {
      if (hash == ZDOVars.s_timeOfDeath) return Helper.PrintDay(ZDOExtraData.s_longs[id][hash]) + " (long)";
      return ZDOExtraData.s_longs[id][hash].ToString() + " (long)";
    }
    if (hasString && (type == "" || type == "string")) return ZDOExtraData.s_strings[id][hash] + " (string)";
    if (hasInt && (type == "" || type == "int")) return ZDOExtraData.s_ints[id][hash].ToString() + " (int)";
    if (hasFloat && (type == "" || type == "float")) return ZDOExtraData.s_floats[id][hash].ToString("F1") + " (float)";
    if (hasId && (type == "" || type == "id")) return ZDOExtraData.s_longs[id][hashId].ToString() + "/" + ZDOExtraData.s_longs[id][hashValue].ToString() + " (id)";
    return "No data";
  }

  public static bool SetData(ZDO zdo, string key, string data, string type) {
    var id = zdo.m_uid;
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    var hasVec = ZDOExtraData.s_vec3.ContainsKey(id) && ZDOExtraData.s_vec3[id].ContainsKey(hash);
    var hasQuat = ZDOExtraData.s_quats.ContainsKey(id) && ZDOExtraData.s_quats[id].ContainsKey(hash);
    var hasLong = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hash);
    var hasString = ZDOExtraData.s_strings.ContainsKey(id) && ZDOExtraData.s_strings[id].ContainsKey(hash);
    var hasInt = ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].ContainsKey(hash);
    var hasFloat = ZDOExtraData.s_floats.ContainsKey(id) && ZDOExtraData.s_floats[id].ContainsKey(hash);
    var hasId = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hashId);

    if (type == "vector" || (type == "" && hasVec)) {
      zdo.Set(hash, Parse.VectorXZY(Parse.Split(data), Vector3.zero));
    } else if (type == "quat" || (type == "" && hasQuat)) {
      zdo.Set(hash, Parse.AngleYXZ(data));
    } else if (type == "long" || (type == "" && hasLong)) {
      if (hash == ZDOVars.s_timeOfDeath) zdo.Set(hash, Helper.ToTick(Parse.Long(data)));
      else zdo.Set(hash, Parse.Long(data));
    } else if (type == "string" || (type == "" && hasString)) {
      zdo.Set(hash, data.Replace('_', ' '));
    } else if (type == "int" || (type == "" && hasInt)) {
      zdo.Set(hash, Parse.Int(data));
    } else if (type == "float" || (type == "" && hasFloat)) {
      zdo.Set(hash, Parse.Float(data));
    } else if (type == "id" || (type == "" && hasId)) {
      var split = Parse.Split(data, '/');
      var hashValue = (key + "_i").GetStableHashCode();
      zdo.Set(hashId, Parse.Long(split[0]));
      zdo.Set(hashValue, Parse.Long(split, 1));
    } else
      return false;
    return true;
  }
  public static bool HasData(ZDO zdo, string key, string data, bool includeEmpty) {
    var id = zdo.m_uid;
    var hash = key.GetStableHashCode();
    var hashId = (key + "_u").GetStableHashCode();
    var hasVec = ZDOExtraData.s_vec3.ContainsKey(id) && ZDOExtraData.s_vec3[id].ContainsKey(hash);
    if (hasVec)
      return Parse.VectorXZYRange(data, Vector3.zero).Includes(ZDOExtraData.s_vec3[id][hash]);
    var hasQuat = ZDOExtraData.s_quats.ContainsKey(id) && ZDOExtraData.s_quats[id].ContainsKey(hash);
    if (hasQuat)
      return Parse.AngleYXZ(data) == ZDOExtraData.s_quats[id][hash];
    var hasLong = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hash);
    if (hasLong) {
      if (hash == ZDOVars.s_timeOfDeath) return Parse.LongRange(data).Includes(Helper.ToDay(ZDOExtraData.s_longs[id][hash]));
      return Parse.LongRange(data).Includes(ZDOExtraData.s_longs[id][hash]);
    }
    var hasString = ZDOExtraData.s_strings.ContainsKey(id) && ZDOExtraData.s_strings[id].ContainsKey(hash);
    if (hasString)
      return data.Replace('_', ' ') == ZDOExtraData.s_strings[id][hash];
    var hasInt = ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].ContainsKey(hash);
    if (hasInt)
      return Parse.IntRange(data).Includes(ZDOExtraData.s_ints[id][hash]);
    var hasFloat = ZDOExtraData.s_floats.ContainsKey(id) && ZDOExtraData.s_floats[id].ContainsKey(hash);
    if (hasFloat)
      return Parse.FloatRange(data).Includes(ZDOExtraData.s_floats[id][hash]);
    var hashValue = (key + "_i").GetStableHashCode();
    var hasId = ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].ContainsKey(hashId) && ZDOExtraData.s_longs[id].ContainsKey(hashValue);
    if (hasId)
      return data == ZDOExtraData.s_longs[id][hashId] + "/" + ZDOExtraData.s_longs[id][hashValue];
    return includeEmpty;
  }
}
