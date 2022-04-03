using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Base class for entity related operations. Provides some utilities.</summary>
public abstract class EntityOperation : BaseOperation {
  protected EntityOperation(Terminal context) : base(context) {
  }
  protected bool Validate(IEnumerable<string> ids) {
    if (ids.Count() == 0) {
      Print("Error: Missing ids");
      return false;
    }
    var invalidIds = ids.Where(id => !id.Contains("*") && ZNetScene.instance.GetPrefab(id) == null);
    if (invalidIds.Count() > 0) {
      Print("Error: Invalid entity ids " + string.Join(", ", invalidIds));
      return false;
    }
    return true;
  }
  private static bool IsIncluded(string id, string name) {
    if (id.StartsWith("*") && id.EndsWith("*")) {
      return name.Contains(id.Substring(1, id.Length - 3));
    }
    if (id.StartsWith("*")) return name.EndsWith(id.Substring(1));
    if (id.EndsWith("*")) return name.StartsWith(id.Substring(0, id.Length - 2));
    return id == name;
  }
  public static IEnumerable<string> GetPrefabs(string id) {
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name));
    else
      values = values.Where(prefab => prefab.name == id);
    return values.Select(prefab => prefab.name);
  }
  public static IEnumerable<ZDO> GetZDOs(string id, FiltererParameters args) {
    var code = id.GetStableHashCode();
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => code == zdo.GetPrefab());
    return FilterZdos(zdos, args);
  }
  public static IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, FiltererParameters args) => args.FilterZdos(zdos);
}
