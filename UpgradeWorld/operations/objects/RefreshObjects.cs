using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Respawns spawners, pickables, chests, etc..</summary>
public class RefreshObjects : EntityOperation {
  public RefreshObjects(Terminal context, List<string> ids, DataParameters args) : base(context) {
    Execute(ids, args);
  }
  private bool SetData(ZDO zdo) {
    var updated = false;
    var spawnId = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
    if (!spawnId.IsNone()) {
      updated = true;
      if (ZDOMan.instance.m_objectsByID.TryGetValue(spawnId, out var spawnedZdo))
        Helper.RemoveZDO(spawnedZdo);
      ZDOExtraData.ReleaseConnection(zdo.m_uid);
      zdo.Set(ZDOVars.s_aliveTime, 0);
    }
    if (zdo.GetLong(ZDOVars.s_pickedTime) != 0) {
      updated = true;
      zdo.Set(ZDOVars.s_pickedTime, 0L);
    }
    if (zdo.GetLong(ZDOVars.s_spawnTime) != 0) {
      updated = true;
      zdo.Set(ZDOVars.s_spawnTime, 0L);
    }
    if (zdo.GetBool(ZDOVars.s_addedDefaultItems)) {
      var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
      if (zdo.GetString(Hash.OverrideItems) != "" || prefab.GetComponent<Container>()?.m_defaultItems.IsEmpty() != true) {
        updated = true;
        zdo.Set(ZDOVars.s_addedDefaultItems, false);
        zdo.Set(ZDOVars.s_items, ClearChest(zdo));
      }
    }
    if (zdo.GetLong(Hash.Changed) != 0) {
      updated = true;
      zdo.Set(Hash.Changed, 0L);
    }
    if (updated) {
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
    }
    return updated;
  }
  private void Execute(List<string> ids, DataParameters args) {
    if (ids.Count == 0) ids.Add("*");
    var prefabs = ids.SelectMany(GetPrefabs).ToList();
    var total = 0;
    var zdos = GetZDOs(args);
    var texts = prefabs.Select(id => {
      var updated = GetZDOs(zdos, id).Where(zdo => SetData(zdo)).Count();
      total += updated;
      if (updated > 0)
        return "Refreshed " + updated + " of " + id + ".";
      return "";
    }).Where(s => s != "").ToArray();
    texts = texts.Prepend($"Refreshed: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
  }

  private string ClearChest(ZDO zdo) {
    var str = zdo.GetString(ZDOVars.s_items);
    if (string.IsNullOrEmpty(str)) return "";
    ZPackage current = new(str);
    ZPackage empty = new();
    empty.Write(current.ReadInt());
    empty.Write(0);
    return empty.GetBase64();
  }
}
