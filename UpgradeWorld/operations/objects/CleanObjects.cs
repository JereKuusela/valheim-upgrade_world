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
  private bool Clean(ZDO zdo, string prefix)
  {
    var zs = ZNetScene.instance;
    var item = zdo.GetString(prefix + "item", "");
    if (item == "") return false;
    if (zs.m_namedPrefabs.ContainsKey(item.GetStableHashCode())) return false;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    zdo.Set(prefix + "item", "");
    zdo.Set(prefix + "variant", 0);
    if (prefix == "")
      zdo.Set(prefix + "quality", 1);
    return true;
  }

  private void Clean(FiltererParameters args)
  {
    var zdos = GetZDOs(args);
    var scene = ZNetScene.instance;
    var zs = ZoneSystem.instance;
    var toRemove = zs.m_locationInstances.Where(x => x.Value.m_location?.m_prefab == null).Select(x => x.Key).ToList();
    foreach (var zone in toRemove)
      zs.m_locationInstances.Remove(zone);
    Print("Removed " + toRemove.Count + " missing locations from the location database.");

    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (zdo.GetPrefab() != LocationProxyHash) continue;
      if (zs.GetLocation(zdo.GetInt(LocationHash)) != null) continue;
      Helper.RemoveZDO(zdo);
      removed++;
    }
    Print("Removed " + removed + " missing locations from the world");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (scene.m_namedPrefabs.ContainsKey(zdo.GetPrefab())) continue;
      Helper.RemoveZDO(zdo);
      removed++;
    }
    Print("Removed " + removed + " missing objects from the world");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (Clean(zdo, ""))
        removed++;
    }
    Print("Removed " + removed + " missing objects from item stands");

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
    Print("Removed " + removed + " missing objects from armor stands");

    removed = 0;
    foreach (var zdo in zdos)
    {
      var items = zdo.GetString(Hash.Items, "");
      if (items == "") continue;
      ZPackage loadPackage = new(zdo.GetString(Hash.Items, ""));
      ZPackage savePackage = new();
      var result = CleanChest(loadPackage, savePackage);
      if (result == 0) continue;
      removed += result;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.instance.GetMyID());
      zdo.Set(Hash.Items, savePackage.GetBase64());
    }
    Print("Removed " + removed + " missing objects from chests");
  }

  private int CleanChest(ZPackage from, ZPackage to)
  {
    int version = from.ReadInt();
    to.Write(version);
    int items = from.ReadInt();
    to.Write(items);
    var removed = 0;
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
      }
    }
    if (removed > 0)
    {
      to.SetPos(4);
      to.Write(items - removed);
    }
    return removed;
  }
}
