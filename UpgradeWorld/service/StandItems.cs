using System.Collections.Generic;

namespace Service;

internal static class StandItems
{
  public static int GetHash(ZDO zdo, string prefix)
  {
    var key = (prefix + "item").GetStableHashCode();
    var hasInt = ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var ints) && ints.TryGetValue(key, out _);
    return ResolveHash(hasInt, zdo.GetInt(key), zdo.GetString(key));
  }

  internal static int ResolveHash(bool hasInt, int current, string legacy) =>
    hasInt ? current : legacy == "" ? 0 : legacy.GetStableHashCode();

  public static IEnumerable<string> Prefixes(ZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    if (prefab == null) yield break;
    if (prefab.GetComponent<ItemStand>() != null) yield return "";
    var armor = prefab.GetComponent<ArmorStand>();
    if (armor != null)
      for (var i = 0; i < armor.m_slots.Count; ++i) yield return i + "_";
  }
}
