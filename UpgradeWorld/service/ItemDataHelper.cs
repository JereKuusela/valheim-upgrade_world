using System.Collections.Generic;
using System.Linq;

namespace Service;

/// <summary>Plain data snapshot of an item, decoupled from ItemDrop.ItemData/prefab lifetime.</summary>
public class ItemRecord
{
  public int PrefabHash;
  public string PrefabName = "";
  public int Stack;
  public float Durability;
  public Vector2i GridPos;
  public bool Equipped;
  public int Quality = 1;
  public int Variant;
  public long CrafterID;
  public string CrafterName = "";
  public Dictionary<string, string> CustomData = [];
  public int WorldLevel;
  public bool PickedUp;
  public bool Cheated;
}

/// <summary>Reads/writes ZDOVars.s_items without instantiating item GameObjects (unlike Inventory.Load/AddItem).</summary>
public static class ItemDataHelper
{
  public static ZPackage? GetPackage(ZDO zdo)
  {
    var bytes = zdo.GetByteArray(ZDOVars.s_items);
    if (bytes.Length > 0) return new ZPackage(bytes);
    var str = zdo.GetString(ZDOVars.s_items, "");
    return str != "" ? new ZPackage(str) : null;
  }

  public static List<ItemRecord> Load(ZDO zdo)
  {
    var pkg = GetPackage(zdo);
    return pkg == null ? [] : Load(pkg);
  }

  public static List<ItemRecord> Load(ZPackage pkg)
  {
    List<ItemRecord> records = [];
    try
    {
      var version = (Version.Item)pkg.ReadInt();
      if (version >= Version.Item.Smaller)
        LoadNew(pkg, version, records);
      else
        LoadOld(pkg, version, records);
    }
    catch { }
    return records;
  }

  private static void LoadNew(ZPackage pkg, Version.Item version, List<ItemRecord> records)
  {
    var count = pkg.ReadUShort();
    for (var i = 0; i < count; i++)
    {
      var (hash, item) = ItemDrop.ItemData.Load(pkg, version);
      // Empty PrefabName means the prefab no longer exists (removed mod/item).
      var prefabName = hash != 0 ? ObjectDB.instance.GetItemPrefab(hash)?.name ?? "" : "";
      records.Add(new ItemRecord
      {
        PrefabHash = hash,
        PrefabName = prefabName,
        Stack = item.m_stack,
        Durability = item.m_durability,
        GridPos = item.m_gridPos,
        Equipped = item.m_equipped,
        Quality = item.m_quality,
        Variant = item.m_variant,
        CrafterID = item.m_crafterID,
        CrafterName = item.m_crafterName,
        CustomData = item.m_customData,
        WorldLevel = item.m_worldLevel,
        PickedUp = item.m_pickedUp,
        Cheated = item.m_cheated,
      });
    }
  }

  private static void LoadOld(ZPackage pkg, Version.Item version, List<ItemRecord> records)
  {
    var count = pkg.ReadInt();
    for (var i = 0; i < count; i++)
    {
      var name = pkg.ReadString();
      var stack = pkg.ReadInt();
      var durability = pkg.ReadSingle();
      var pos = pkg.ReadVector2i();
      var equipped = pkg.ReadBool();
      var quality = version >= Version.Item.Quality ? pkg.ReadInt() : 1;
      var variant = version >= Version.Item.Variant ? pkg.ReadInt() : 0;
      var crafterID = 0L;
      var crafterName = "";
      if (version >= Version.Item.CrafterID)
      {
        crafterID = pkg.ReadLong();
        crafterName = pkg.ReadString();
      }
      Dictionary<string, string> customData = [];
      if (version >= Version.Item.CustomData)
      {
        var dataCount = pkg.ReadInt();
        for (var j = 0; j < dataCount; j++)
          customData[pkg.ReadString()] = pkg.ReadString();
      }
      var worldLevel = version >= Version.Item.WorldLevel ? pkg.ReadInt() : 0;
      var pickedUp = version >= Version.Item.PickedUp && pkg.ReadBool();
      var cheated = version == Version.Item.AbandonedDN && pkg.ReadBool();

      // Empty PrefabName means the prefab no longer exists (removed mod/item).
      var hash = name != "" ? name.GetStableHashCode() : 0;
      var prefabName = hash != 0 ? ObjectDB.instance.GetItemPrefab(hash)?.name ?? "" : "";
      records.Add(new ItemRecord
      {
        PrefabHash = hash,
        PrefabName = prefabName,
        Stack = stack,
        Durability = durability,
        GridPos = pos,
        Equipped = equipped,
        Quality = quality,
        Variant = variant,
        CrafterID = crafterID,
        CrafterName = crafterName,
        CustomData = customData,
        WorldLevel = worldLevel,
        PickedUp = pickedUp,
        Cheated = cheated,
      });
    }
  }

  public static int CountInvalid(List<ItemRecord> records) => records.Count(r => r.PrefabHash == 0);

  public static List<ItemRecord> RemoveInvalid(List<ItemRecord> records) => [.. records.Where(r => r.PrefabHash != 0)];

  public static byte[] Save(List<ItemRecord> records)
  {
    ZPackage pkg = new();
    Save(records, pkg);
    return pkg.GetArray();
  }

  public static void Save(List<ItemRecord> records, ZPackage pkg)
  {
    pkg.Write((int)Version.Item.ChunksNCheats);
    pkg.Write((ushort)records.Count);
    foreach (var record in records)
    {
      ItemDrop.ItemData item = new()
      {
        m_dropPrefab = ObjectDB.instance.GetItemPrefab(record.PrefabHash),
        m_stack = record.Stack,
        m_durability = record.Durability,
        m_gridPos = record.GridPos,
        m_equipped = record.Equipped,
        m_quality = record.Quality,
        m_variant = record.Variant,
        m_crafterID = record.CrafterID,
        m_crafterName = record.CrafterName,
        m_customData = record.CustomData,
        m_worldLevel = record.WorldLevel,
        m_pickedUp = record.PickedUp,
        m_cheated = record.Cheated,
      };
      item.Save(pkg);
    }
  }
}
