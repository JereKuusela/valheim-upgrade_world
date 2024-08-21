using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Respawns spawners, pickables, chests, etc..</summary>
public class RefreshObjects : EntityOperation
{
  public RefreshObjects(Terminal context, List<string> ids, DataParameters args) : base(context, args.Pin)
  {
    Execute(ids, args);
  }
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
      AddPin(zdo.GetPosition());
    }
    return updated;
  }
  private void Execute(List<string> ids, DataParameters args)
  {
    var prefabs = GetPrefabs(ids, args.Types);
    var zdos = GetZDOs(args, prefabs);
    var total = 0;
    var counts = prefabs.ToDictionary(prefab => prefab, prefab => 0);
    foreach (var zdo in zdos)
    {
      if (!SetData(zdo))
        continue;
      counts[zdo.m_prefab] += 1;
      total += 1;
    }
    var linq = counts.Where(kvp => kvp.Value > 0).Select(kvp => $"Refreshed {kvp.Value} of {GetName(kvp.Key)}.");
    string[] texts = [$"Refreshed: {total}", .. linq];
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }

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
