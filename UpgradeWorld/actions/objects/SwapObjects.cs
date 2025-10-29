using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Swaps objects with another one.</summary>
public class SwapObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : ExecutedEntityOperation(context, ids, args)
{
  private int ToSwap = 0;

  protected override HashSet<int> GetPrefabsForOperation()
  {
    ToSwap = Ids.FirstOrDefault().GetStableHashCode();
    return EntityOperation.GetPrefabs(Ids.Skip(1).ToList(), Args.Types);
  }

  protected override bool ProcessZDO(ZDO zdo)
  {
    if (zdo.m_prefab == ToSwap) return false;

    ZDOData data = new(zdo) { Prefab = ToSwap };
    data.Clone();
    Helper.RemoveZDO(zdo);
    return true;
  }

  protected override string GetNoObjectsMessage() => "No objects found to swap.";

  protected override string GetInitMessage() => $"Swapping {TotalCount} object{(TotalCount > 1 ? "s" : "")}";

  protected override string GetProcessedMessage() => $"Swapped: {ProcessedCount}";

  protected override string GetCountMessage(int count, int prefab) => $"Swapped {count} of {EntityOperation.GetName(prefab)}.";
}
