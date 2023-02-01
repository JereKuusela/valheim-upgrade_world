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
  private static int PlayerHash = "Player".GetStableHashCode();
  public static List<string> GetPrefabs(string id)
  {
    IEnumerable<KeyValuePair<int, GameObject>> values = ZNetScene.instance.m_namedPrefabs.Where(kvp => kvp.Key != PlayerHash);
    if (id == "*")
    {
      // Empty on purpose.
    }
    else if (id.Contains("*"))
      values = values.Where(kvp => IsIncluded(id, kvp.Value.name));
    else
      values = values.Where(kvp => string.Equals(kvp.Value.name, id, StringComparison.OrdinalIgnoreCase));
    return values.Select(kvp => kvp.Value.name).ToList();
  }
  public static ZDO[] GetZDOs(string id, DataParameters args)
  {
    var code = id.GetStableHashCode();
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => code == zdo.GetPrefab());
    return FilterZdos(zdos, args).ToArray();
  }
  public static ZDO[] GetZDOs(ZDO[] zdos, string id)
  {
    var code = id.GetStableHashCode();
    return zdos.Where(zdo => code == zdo.GetPrefab()).ToArray();
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
