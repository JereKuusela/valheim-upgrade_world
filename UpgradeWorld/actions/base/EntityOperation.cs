using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Base class for entity related operations. Provides some utilities.</summary>
public abstract class EntityOperation(Terminal context, bool pin = false) : BaseOperation(context, pin)
{
  private static bool IsIncluded(string id, string name)
  {
    if (id == "*") return true;
    if (id[0] == '*' && id[id.Length - 1] == '*')
    {
      return name.Contains(id.Substring(1, id.Length - 2));
    }
    if (id[0] == '*') return name.EndsWith(id.Substring(1), StringComparison.OrdinalIgnoreCase);
    if (id[id.Length - 1] == '*') return name.StartsWith(id.Substring(0, id.Length - 1), StringComparison.OrdinalIgnoreCase);
    return id == name;
  }
  private static readonly int PlayerHash = "Player".GetStableHashCode();
  private static Dictionary<int, string> HashToName = [];
  public static string GetName(int prefab) => HashToName.TryGetValue(prefab, out var name) ? name : "";
  public static HashSet<int> GetPrefabs(IEnumerable<string> id, List<string[]> types) => id.Count() == 0 ? GetPrefabs("*", types) : id.SelectMany(id => GetPrefabs(id, types)).ToHashSet();
  public static HashSet<int> GetPrefabs(string id, List<string[]> types)
  {
    if (HashToName.Count == 0)
      HashToName = ZNetScene.instance.m_namedPrefabs.ToDictionary(kvp => kvp.Value.name.GetStableHashCode(), kvp => kvp.Value.name);
    IEnumerable<KeyValuePair<int, GameObject>> values = ZNetScene.instance.m_namedPrefabs.Where(kvp => kvp.Key != PlayerHash);
    if (id == "*")
    {
      // Empty on purpose.
    }
    else if (id.Contains("*"))
      values = values.Where(kvp => IsIncluded(id, kvp.Value.name));
    else
      values = values.Where(kvp => string.Equals(kvp.Value.name, id, StringComparison.OrdinalIgnoreCase));

    if (types.Count > 0)
      values = ComponentInfo.HaveComponent(values.ToArray(), types);
    return values.Select(kvp => kvp.Key).ToHashSet();
  }
  public static ZDO[] GetZDOs(ZDO[] zdos, int hash)
  {
    return zdos.Where(zdo => hash == zdo.m_prefab).ToArray();
  }
  public static ZDO[] GetZDOs(FiltererParameters args, HashSet<int> prefabs)
  {
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => prefabs.Contains(zdo.m_prefab));
    return FilterZdos(zdos, args).ToArray();
  }
  public static ZDO[] GetZDOs(DataParameters args, HashSet<int> prefabs)
  {
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => prefabs.Contains(zdo.m_prefab));
    return FilterZdos(zdos, args).ToArray();
  }
  public static ZDO[] GetZDOs(FiltererParameters args) => FilterZdos(ZDOMan.instance.m_objectsByID.Values, args).ToArray();
  public static ZDO[] GetZDOs(DataParameters args) => FilterZdos(ZDOMan.instance.m_objectsByID.Values, args).ToArray();
  public static ZDO[] GetZDOs(string id) => GetZDOs(id.GetStableHashCode());
  public static ZDO[] GetZDOs(int hash) => ZDOMan.instance.m_objectsByID.Values.Where(zdo => hash == zdo.m_prefab).ToArray();

  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, DataParameters args) => args.LimitZdos(args.FilterZdos(zdos, false));
  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, FiltererParameters args) => args.LimitZdos(args.FilterZdos(zdos, false));
}
