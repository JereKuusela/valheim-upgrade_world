using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Swaps locations with another one.</summary>
public class SwapLocations : EntityOperation
{
  public SwapLocations(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
  {
    Swap(ids, args);
  }
  private void Swap(IEnumerable<string> ids, DataParameters args)
  {
    var toSwap = ids.FirstOrDefault().GetStableHashCode();
    var prefabs = ids.Skip(1).Select(id => id.GetStableHashCode()).ToHashSet();
    var swappedObjects = 0;
    var zdos = GetZDOs(args).Where(zdo => LocationProxyHash == zdo.GetPrefab()).ToArray();
    foreach (var zdo in zdos)
    {
      if (!args.Roll()) continue;
      if (!prefabs.Contains(zdo.GetInt(LocationHash))) continue;
      swappedObjects++;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.instance.GetMyID());
      zdo.Set(LocationHash, toSwap);
      Refresh(zdo);
    }
    var locs = ZoneSystem.instance.m_locationInstances;
    var location = ZoneSystem.instance.m_locationsByHash[toSwap];
    var toModify = locs.Where(kvp => prefabs.Contains(kvp.Value.m_location?.m_hash ?? 0)).ToArray();
    foreach (var zone in toModify)
    {
      var data = locs[zone.Key];
      data.m_location = location;
      locs[zone.Key] = data;
    }
    var swappedDatabase = toModify.Length;
    Print($"Swapped {swappedObjects} location objects and {swappedDatabase} location entries.", false);
  }

  private static void Refresh(ZDO zdo)
  {
    if (!ZNetScene.instance.m_instances.TryGetValue(zdo, out var view)) return;
    var newObj = ZNetScene.instance.CreateObject(zdo);
    UnityEngine.Object.Destroy(view.gameObject);
    ZNetScene.instance.m_instances[zdo] = newObj.GetComponent<ZNetView>();
  }
}
