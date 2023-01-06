using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Base class for entity related operations. Provides some utilities.</summary>
public abstract class EntityOperation : BaseOperation
{
  protected EntityOperation(Terminal context) : base(context)
  {
  }
  private static bool IsIncluded(string id, string name)
  {
    if (id == "*") return true;
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.OrdinalIgnoreCase))
    {
      return name.Contains(id.Substring(1, id.Length - 2));
    }
    if (id.StartsWith("*", StringComparison.Ordinal)) return name.EndsWith(id.Substring(1), StringComparison.OrdinalIgnoreCase);
    if (id.EndsWith("*", StringComparison.Ordinal)) return name.StartsWith(id.Substring(0, id.Length - 1), StringComparison.OrdinalIgnoreCase);
    return id == name;
  }
  public static List<string> GetPrefabs(string id)
  {
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name));
    else
      values = values.Where(prefab => string.Equals(prefab.name, id, StringComparison.OrdinalIgnoreCase));
    return values.Select(prefab => prefab.name).ToList();
  }
  public static ZDO[] GetZDOs(string id, DataParameters args)
  {
    var code = id.GetStableHashCode();
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => code == zdo.GetPrefab());
    return FilterZdos(zdos, args).ToArray();
  }
  public static ZDO[] GetZDOs(FiltererParameters args)
  {
    var zdos = ZDOMan.instance.m_objectsByID.Values;
    return FilterZdos(zdos, args).ToArray();
  }
  public static ZDO[] GetZDOs(string id)
  {
    var code = id.GetStableHashCode();
    return ZDOMan.instance.m_objectsByID.Values.Where(zdo => code == zdo.GetPrefab()).ToArray();
  }
  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, DataParameters args) => args.FilterZdos(zdos);
  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, FiltererParameters args) => args.FilterZdos(zdos);
}
