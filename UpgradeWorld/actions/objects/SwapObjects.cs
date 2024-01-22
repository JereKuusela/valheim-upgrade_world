using System.Collections.Generic;
using System.Linq;
using Service;
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
    var prefabs = GetPrefabs(ids.Skip(1).ToList(), args.Types);
    var allZdos = GetZDOs(args, prefabs);
    var total = 0;
    var counts = prefabs.ToDictionary(prefab => prefab, prefab => 0);
    foreach (var zdo in allZdos)
    {
      if (!args.Roll()) continue;
      if (zdo.m_prefab == toSwap) continue;
      counts[zdo.m_prefab] += 1;
      total += 1;
      ZDOData data = new(zdo) { Prefab = toSwap };
      data.Clone();
      AddPin(zdo.GetPosition());
      Helper.RemoveZDO(zdo);
    }
    var linq = counts.Where(kvp => kvp.Value > 0).Select(kvp => $"Swapped {kvp.Value} of {GetName(kvp.Key)}.");
    string[] texts = [$"Swapped: {total}", .. linq];
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
