using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Removes missing objects.</summary>
public class CleanObjects : EntityOperation
{
  public CleanObjects(Terminal context, FiltererParameters args) : base(context)
  {
    Clean(args);
  }

  private void Clean(FiltererParameters args)
  {
    var zdos = GetZDOs(args);
    var scene = ZNetScene.instance;
    var zs = ZoneSystem.instance;
    var toRemove = zs.m_locationInstances.Where(x => x.Value.m_location?.m_prefab == null).Select(x => x.Key).ToList();
    foreach (var zone in toRemove)
      zs.m_locationInstances.Remove(zone);
    if (toRemove.Count > 0)
      Print($"Removed {toRemove.Count} missing location entries.");

    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (zdo.GetPrefab() != LocationProxyHash) continue;
      if (zs.GetLocation(zdo.GetInt(LocationHash)) != null) continue;
      Helper.RemoveZDO(zdo);
      removed++;
    }
    if (removed > 0)
      Print($"Removed {removed} missing location objects.");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (scene.m_namedPrefabs.ContainsKey(zdo.GetPrefab())) continue;
      Helper.RemoveZDO(zdo);
      removed++;
    }
    if (removed > 0)
      Print($"Removed {removed} missing objects.");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (Clean(zdo, ""))
        removed++;
    }
    if (removed > 0)
      Print($"Removed {removed} missing objects from item stands");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (Clean(zdo, "0_"))
        removed++;
      if (Clean(zdo, "1_"))
        removed++;
      if (Clean(zdo, "2_"))
        removed++;
      if (Clean(zdo, "3_"))
        removed++;
      if (Clean(zdo, "4_"))
        removed++;
      if (Clean(zdo, "5_"))
        removed++;
      if (Clean(zdo, "6_"))
        removed++;
      if (Clean(zdo, "7_"))
        removed++;
      if (Clean(zdo, "8_"))
        removed++;
      if (Clean(zdo, "9_"))
        removed++;
    }
    if (removed > 0)
      Print($"Removed {removed} missing objects from armor stands");

    removed = 0;
    foreach (var zdo in zdos)
    {
      var items = zdo.GetString(ZDOVars.s_items);
      if (items == "") continue;
      ZPackage loadPackage = new(items);
      ZPackage savePackage = new();
      var result = CleanChest(loadPackage, savePackage);
      if (result == 0) continue;
      removed += result;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.Set(ZDOVars.s_items, savePackage.GetBase64());
    }
    if (removed > 0)
      Print($"Removed {removed} missing objects from chests");


    var prefab = zs.m_zoneCtrlPrefab;
    var zoneCtrlHash = prefab.name.GetStableHashCode();
    var reseted = 0;
    var count = 0;
    foreach (var zdo in zdos)
    {
      if (zdo.GetPrefab() != zoneCtrlHash) continue;
      var id = zdo.m_uid;
      var longs = ZDOExtraData.GetLongs(id);
      if (longs.Count < 1) continue;
      count += longs.Count;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
      ZDOHelper.Release(ZDOExtraData.s_longs, id);
      zdo.IncreaseDataRevision();
      reseted++;
    }
    if (count > 0)
      Print($"Cleared {count} spawn data from {reseted} zone controls.");

    var updated = 0;
    foreach (var zdo in zdos)
    {
      var rooms = zdo.GetInt(ZDOVars.s_rooms);
      if (rooms == 0) continue;
      ZDOMan.instance.ConvertDungeonRooms([zdo]);
      Print($"Optimized dungeon at {Helper.PrintVectorXZY(zdo.GetPosition())}.");
      updated++;
    }
    if (updated > 0)
      Print($"Optimized {updated} dungeons.");
    Print("World cleaned.");
  }

  private bool Clean(ZDO zdo, string prefix)
  {
    var zs = ZNetScene.instance;
    var item = zdo.GetString(prefix + "item", "");
    if (item == "") return false;
    if (zs.m_namedPrefabs.ContainsKey(item.GetStableHashCode())) return false;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.GetSessionID());
    zdo.Set(prefix + "item", "");
    zdo.Set(prefix + "variant", 0);
    if (prefix == "")
      zdo.Set(prefix + "quality", 1);
    return true;
  }

  private int CleanChest(ZPackage from, ZPackage to)
  {
    int version = from.ReadInt();
    to.Write(version);
    int items = from.ReadInt();
    to.Write(items);
    var removed = 0;
    try
    {
      for (int i = 0; i < items; i++)
      {
        string text = from.ReadString();
        if (ZNetScene.instance.m_namedPrefabs.ContainsKey(text.GetStableHashCode()))
        {
          to.Write(text);
          to.Write(from.ReadInt());
          to.Write(from.ReadSingle());
          to.Write(from.ReadVector2i());
          to.Write(from.ReadBool());
          if (version >= 101)
            to.Write(from.ReadInt());
          if (version >= 102)
            to.Write(from.ReadInt());
          if (version >= 103)
          {
            to.Write(from.ReadLong());
            to.Write(from.ReadString());
          }
          if (version >= 104)
          {
            var dataAmount = from.ReadInt();
            to.Write(dataAmount);
            for (int j = 0; j < dataAmount; j++)
            {
              to.Write(from.ReadString());
              to.Write(from.ReadString());
            }
          }
          if (version >= 105)
            to.Write(from.ReadInt());
          if (version >= 106)
            to.Write(from.ReadBool());
        }
        else
        {
          removed++;
          from.ReadInt();
          from.ReadSingle();
          from.ReadVector2i();
          from.ReadBool();
          if (version >= 101)
            from.ReadInt();
          if (version >= 102)
            from.ReadInt();
          if (version >= 103)
          {
            from.ReadLong();
            from.ReadString();
          }
          if (version >= 104)
          {
            var dataAmount = from.ReadInt();
            for (int j = 0; j < dataAmount; j++)
            {
              from.ReadString();
              from.ReadString();
            }
          }
          if (version >= 105)
            from.ReadInt();
          if (version >= 106)
            from.ReadBool();
        }
      }
    }
    catch
    {
      // Fallback for truly corrupted chests.
      removed = items;
    }


    if (removed > 0)
    {
      to.SetPos(4);
      to.Write(items - removed);
    }
    return removed;
  }
}
