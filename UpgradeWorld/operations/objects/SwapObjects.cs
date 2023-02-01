using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Swaps objects with another one.</summary>
public class SwapObjects : EntityOperation
{
  public SwapObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
  {
    Swap(ids, args);
  }
  private void Swap(IEnumerable<string> ids, DataParameters args)
  {
    var toSwap = ids.FirstOrDefault().GetStableHashCode();
    var prefabs = ids.Skip(1).SelectMany(GetPrefabs).ToList();
    var total = 0;
    var allZdos = GetZDOs(args);
    var texts = prefabs.Select(id =>
    {
      var zdos = GetZDOs(allZdos, id);
      var swapped = 0;
      foreach (var zdo in zdos)
      {
        if (!args.Roll()) continue;
        if (zdo.GetPrefab() == toSwap) continue;
        swapped++;
        if (!zdo.IsOwner())
          zdo.SetOwner(ZDOMan.instance.GetMyID());
        zdo.SetPrefab(toSwap);
        Refresh(zdo);
      }
      total += swapped;
      return "Swapped " + swapped + " of " + id + ".";
    });
    texts = texts.Prepend($"Total: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
  }

  private static void Refresh(ZDO zdo)
  {
    if (!ZNetScene.instance.m_instances.TryGetValue(zdo, out var view)) return;
    var newObj = ZNetScene.instance.CreateObject(zdo);
    UnityEngine.Object.Destroy(view.gameObject);
    ZNetScene.instance.m_instances[zdo] = newObj.GetComponent<ZNetView>();
  }
}
