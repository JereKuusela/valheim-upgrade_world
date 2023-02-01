using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Respawns spawners, pickables, chests, etc..</summary>
public class RefreshObjects : EntityOperation
{
  public RefreshObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
  {
    Execute(ids, args);
  }
  private bool SetData(ZDO zdo, List<string> datas)
  {
    var updated = false;
    if (zdo.GetZDOID(Hash.SpawnId) != ZDOID.None)
    {
      updated = true;
      if (ZDOMan.instance.m_objectsByID.TryGetValue(zdo.GetZDOID(Hash.SpawnId), out var spawnedZdo))
        Helper.RemoveZDO(spawnedZdo);
      zdo.Set(Hash.SpawnId, ZDOID.None);
      zdo.Set(Hash.AliveTime, 0);
    }
    if (zdo.GetLong(Hash.PickedTime) != 0)
    {
      updated = true;
      zdo.Set(Hash.PickedTime, 0L);
    }
    if (zdo.GetLong(Hash.SpawnTime) != 0)
    {
      updated = true;
      zdo.Set(Hash.SpawnTime, 0L);
    }
    if (zdo.GetBool(Hash.AddedDefaultItems))
    {
      var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
      if (zdo.GetString(Hash.OverrideItems) != "" || prefab.GetComponent<Container>()?.m_defaultItems.IsEmpty() != true)
      {
        updated = true;
        zdo.Set(Hash.AddedDefaultItems, false);
        zdo.Set(Hash.Items, ClearChest(zdo));
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
        zdo.SetOwner(ZDOMan.instance.GetMyID());
      zdo.IncreseDataRevision();
    }
    return updated;
  }
  private void Execute(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = ids.SelectMany(GetPrefabs).ToList();
    var total = 0;
    var texts = prefabs.Select(id =>
    {
      var updated = GetZDOs(id, args).Where(zdo => SetData(zdo, args.Datas)).Count();
      total += updated;
      if (updated > 0)
        return "Refreshed " + updated + " of " + id + ".";
      return "";
    }).Where(s => s != "").ToArray();
    texts = texts.Prepend($"Refreshed: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
  }

  private string ClearChest(ZDO zdo)
  {
    var str = zdo.GetString(Hash.Items);
    if (string.IsNullOrEmpty(str)) return "";
    ZPackage current = new ZPackage(str);
    ZPackage empty = new();
    empty.Write(current.ReadInt());
    empty.Write(0);
    return empty.GetBase64();
  }
}
