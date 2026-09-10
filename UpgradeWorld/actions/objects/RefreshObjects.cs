using System.Collections.Generic;
using Service;
namespace UpgradeWorld;
/// <summary>Respawns spawners, pickables, chests, etc..</summary>
public class RefreshObjects(Terminal context, HashSet<string> ids, DataParameters args) : ExecutedEntityOperation(context, ids, args)
{
  private bool SetData(ZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    var container = prefab != null ? prefab.GetComponent<Container>() : null;
    var resetChest = container != null && zdo.GetBool(ZDOVars.s_addedDefaultItems) &&
      (zdo.GetString(Hash.OverrideItems) != "" || !container.m_defaultItems.IsEmpty());
    if (resetChest)
    {
      if (!ChestInventory.CanModify(zdo, out var reason) ||
          !ChestInventory.TryRead(zdo, out _, out reason))
      {
        Print($"Skipping chest {zdo.m_uid} (unchanged): {reason}");
        return false;
      }
    }
    var updated = resetChest;
    var spawnId = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
    if (!spawnId.IsNone())
    {
      updated = true;
      if (ZDOMan.instance.m_objectsByID.TryGetValue(spawnId, out var spawnedZdo))
        Helper.RemoveZDO(spawnedZdo);
      ZDOExtraData.ReleaseConnection(zdo.m_uid);
      zdo.Set(ZDOVars.s_aliveTime, 0);
    }
    if (zdo.GetLong(ZDOVars.s_pickedTime) != 0)
    {
      updated = true;
      zdo.Set(ZDOVars.s_pickedTime, 0L);
    }
    if (zdo.GetLong(ZDOVars.s_spawnTime) != 0)
    {
      updated = true;
      zdo.Set(ZDOVars.s_spawnTime, 0L);
    }
    if (zdo.GetLong(Hash.Changed) != 0)
    {
      updated = true;
      zdo.Set(Hash.Changed, 0L);
    }
    if (updated)
    {
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
    }
    // Container rolls default items in Awake, not in CheckForChanges. Recreate
    // it so already-loaded chests reroll too, using the native/modded loot path.
    if (resetChest) ChestInventory.Respawn(zdo);
    return updated;
  }
  protected override bool ProcessZDO(ZDO zdo) => SetData(zdo);

  protected override string GetNoObjectsMessage() => "No objects found to refresh.";

  protected override string GetInitMessage() => $"Refreshing {TotalCount} object{(TotalCount > 1 ? "s" : "")}. Treasure chest contents are replaced without an item allowlist.";

  protected override string GetProcessedMessage() => $"Refreshed: {ProcessedCount}";

  protected override string GetCountMessage(int count, int prefab) => $"Refreshed {count} of {EntityOperation.GetName(prefab)}.";

}
