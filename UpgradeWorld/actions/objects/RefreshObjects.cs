using System.Collections.Generic;
using Service;
namespace UpgradeWorld;
/// <summary>Respawns spawners, pickables, chests, etc..</summary>
public class RefreshObjects(Terminal context, HashSet<string> ids, DataParameters args) : ExecutedEntityOperation(context, ids, args)
{
  private bool SetData(ZDO zdo)
  {
    var updated = false;
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
    if (zdo.GetBool(ZDOVars.s_addedDefaultItems))
    {
      var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
      if (zdo.GetString(Hash.OverrideItems) != "" || prefab.GetComponent<Container>()?.m_defaultItems.IsEmpty() != true)
      {
        updated = true;
        zdo.Set(ZDOVars.s_addedDefaultItems, false);
        zdo.Set(ZDOVars.s_items, ClearChest(zdo));
      }
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
    return updated;
  }
  protected override bool ProcessZDO(ZDO zdo) => SetData(zdo);

  protected override string GetNoObjectsMessage() => "No objects found to refresh.";

  protected override string GetInitMessage() => $"Refreshing {TotalCount} object{(TotalCount > 1 ? "s" : "")}.";

  protected override string GetProcessedMessage() => $"Refreshed: {ProcessedCount}";

  protected override string GetCountMessage(int count, int prefab) => $"Refreshed {count} of {EntityOperation.GetName(prefab)}.";

  private string ClearChest(ZDO zdo)
  {
    var str = zdo.GetString(ZDOVars.s_items);
    if (string.IsNullOrEmpty(str)) return "";
    ZPackage current = new(str);
    ZPackage empty = new();
    empty.Write(current.ReadInt());
    empty.Write(0);
    return empty.GetBase64();
  }
}
