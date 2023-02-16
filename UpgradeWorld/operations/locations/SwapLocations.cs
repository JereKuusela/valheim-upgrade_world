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
    var total = 0;
    var zdos = GetZDOs(args).Where(zdo => LocationProxyHash == zdo.GetPrefab()).ToArray();
    foreach (var zdo in zdos)
    {
      if (!args.Roll()) continue;
      if (!prefabs.Contains(zdo.GetInt(LocationHash))) continue;
      total++;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.instance.GetMyID());
      zdo.Set(LocationHash, toSwap);
      Refresh(zdo);
    }
    foreach (var kvp in ZoneSystem.instance.m_locationInstances)
    {
      var location = kvp.Value;
      if (!prefabs.Contains(location.m_location?.m_hash ?? 0)) continue;
      location.m_location = ZoneSystem.instance.m_locationsByHash[toSwap];
      ZoneSystem.instance.m_locationInstances[kvp.Key] = location;
    }
    Print($"Swapped {total} locations.", false);
  }

  private static void Refresh(ZDO zdo)
  {
    if (!ZNetScene.instance.m_instances.TryGetValue(zdo, out var view)) return;
    var newObj = ZNetScene.instance.CreateObject(zdo);
    UnityEngine.Object.Destroy(view.gameObject);
    ZNetScene.instance.m_instances[zdo] = newObj.GetComponent<ZNetView>();
  }
}
