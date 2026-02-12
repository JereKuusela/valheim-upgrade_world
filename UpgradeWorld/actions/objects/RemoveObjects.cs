using System.Collections.Generic;

namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : ExecutedEntityOperation(context, ids, args)
{

  protected override bool ProcessZDO(ZDO zdo)
  {
    // Have to check if already removed.
    if (zdo.m_prefab == 0)
      return false;
    Helper.RemoveZDO(zdo);
    return true;
  }

  protected override string GetNoObjectsMessage() => "No objects found to remove.";

  protected override string GetInitMessage() => $"Removing {TotalCount} object{(TotalCount > 1 ? "s" : "")}.";

  protected override string GetProcessedMessage() => $"Removed: {ProcessedCount}";

  protected override string GetCountMessage(int count, int prefab) => $"Removed {count} of {EntityOperation.GetName(prefab)}.";
}
