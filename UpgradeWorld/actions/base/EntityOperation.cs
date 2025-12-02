using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Base class for entity related operations. Provides some utilities.</summary>
public abstract class EntityOperation(Terminal context, bool pin) : BaseOperation(context, pin)
{
  protected static string S(int i) => i > 1 ? "s" : "";

  private static readonly int PlayerHash = "Player".GetStableHashCode();
  private static Dictionary<int, string> HashToName = [];
  public static string GetName(int prefab) => HashToName.TryGetValue(prefab, out var name) ? name : "";
  public static HashSet<int> GetPrefabs(IEnumerable<string> id, List<string[]> types) => id.Count() == 0 ? GetPrefabs("*", types) : [.. id.SelectMany(id => GetPrefabs(id, types))];
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
      values = values.Where(kvp => Helper.IsIncluded(id, kvp.Value.name));
    else
      values = values.Where(kvp => string.Equals(kvp.Value.name, id, StringComparison.OrdinalIgnoreCase));

    if (types.Count > 0)
      values = ComponentInfo.HaveComponent(values.ToArray(), types);
    return [.. values.Select(kvp => kvp.Key)];
  }
  public static ZDO[] GetZDOs(ZDO[] zdos, int hash)
  {
    return [.. zdos.Where(zdo => hash == zdo.m_prefab)];
  }
  public static ZDO[] GetZDOs(FiltererParameters args, HashSet<int> prefabs)
  {
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => prefabs.Contains(zdo.m_prefab));
    return [.. FilterZdos(zdos, args)];
  }
  public static ZDO[] GetZDOs(DataParameters args, HashSet<int> prefabs)
  {
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => prefabs.Contains(zdo.m_prefab));
    return [.. FilterZdos(zdos, args)];
  }
  public static ZDO[] GetZDOs(FiltererParameters args) => [.. FilterZdos(ZDOMan.instance.m_objectsByID.Values, args)];
  public static ZDO[] GetZDOs(DataParameters args) => [.. FilterZdos(ZDOMan.instance.m_objectsByID.Values, args)];
  public static ZDO[] GetZDOs(string id) => GetZDOs(id.GetStableHashCode());
  public static ZDO[] GetZDOs(int hash) => [.. ZDOMan.instance.m_objectsByID.Values.Where(zdo => hash == zdo.m_prefab)];

  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, DataParameters args) => args.LimitZdos(args.FilterZdos(zdos, false));
  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, FiltererParameters args) => args.LimitZdos(args.FilterZdos(zdos, false));
}
