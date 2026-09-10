using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Service;

// Inspect serialized records without Inventory.Load: that method silently drops
// missing prefabs, which would make a reset allowlist unsafe.
internal sealed class ChestInventory
{
  internal sealed class Entry
  {
    public int Hash;
    public string Name = "";
    public int Stack;
    public int Quality;
    public int Variant;
    public byte[] Record = [];
  }

  public int Version;
  public readonly List<Entry> Items = [];

  public static bool TryRead(ZDO zdo, out ChestInventory inventory, out string error) =>
    TryRead(zdo.GetByteArray(ZDOVars.s_items), zdo.GetString(ZDOVars.s_items), out inventory, out error);

  internal static bool TryRead(byte[]? bytes, string legacy, out ChestInventory inventory, out string error)
  {
    inventory = new();
    error = "";
    try
    {
      // The presence of bytes is authoritative, even if malformed. Never fall
      // back to a stale text inventory after a failed byte-inventory read.
      if (bytes == null && legacy == "") return true;
      bytes ??= Convert.FromBase64String(legacy);
      ZPackage package = new(bytes);
      inventory.Version = package.ReadInt();
      var version = inventory.Version;
      if (version < 100 || version > 109)
        throw new InvalidDataException($"Unsupported inventory version {version}");
      var count = version >= 108 ? package.ReadUShort() : package.ReadInt();
      if (count < 0 || count > bytes.Length)
        throw new InvalidDataException("Invalid inventory count");
      for (var i = 0; i < count; ++i)
      {
        var start = package.GetPos();
        Entry entry;
        if (version >= 108)
        {
          var (hash, item) = ItemDrop.ItemData.Load(package, (global::Version.Item)version);
          entry = new() { Hash = hash, Stack = item.m_stack, Quality = item.m_quality, Variant = item.m_variant };
        }
        else
          entry = ReadLegacy(package, version);
        var length = package.GetPos() - start;
        entry.Record = new byte[length];
        Buffer.BlockCopy(bytes, start, entry.Record, 0, length);
        inventory.Items.Add(entry);
      }
      if (package.GetPos() != bytes.Length)
        throw new InvalidDataException("Unrecognized trailing inventory data");
      return true;
    }
    catch (Exception e) when (e is IOException || e is InvalidDataException || e is ArgumentException || e is FormatException || e is OverflowException)
    {
      error = e.Message;
      inventory = new();
      return false;
    }
  }

  private static Entry ReadLegacy(ZPackage p, int version)
  {
    var name = p.ReadString();
    Entry item = new() { Name = name, Hash = name.GetStableHashCode(), Stack = p.ReadInt(), Quality = 1 };
    p.ReadSingle(); // durability
    p.ReadVector2i();
    p.ReadBool(); // equipped
    if (version >= 101) item.Quality = p.ReadInt();
    if (version >= 102) item.Variant = p.ReadInt();
    if (version >= 103) { p.ReadLong(); p.ReadString(); }
    if (version >= 104)
    {
      var count = p.ReadInt();
      if (count < 0 || count > p.Size() - p.GetPos())
        throw new InvalidDataException("Invalid custom data count");
      for (var i = 0; i < count; ++i) { p.ReadString(); p.ReadString(); }
    }
    if (version >= 105) p.ReadInt(); // world level
    if (version >= 106) p.ReadBool(); // picked up
    if (version == 107) p.ReadBool(); // cheated (pre-packed format)
    return item;
  }

  // Preserve surviving records byte-for-byte, including mod custom data,
  // durability, crafter, grid positions and cheat flags. Only count changes.
  public byte[] Keep(Func<Entry, bool> keep)
  {
    var items = Items.Where(keep).ToArray();
    using MemoryStream stream = new();
    using BinaryWriter writer = new(stream);
    writer.Write(Version);
    if (Version >= 108) writer.Write(checked((ushort)items.Length));
    else writer.Write(items.Length);
    foreach (var item in items) writer.Write(item.Record);
    return stream.ToArray();
  }

  public static bool CanModify(ZDO zdo, out string reason)
  {
    reason = "";
    if (!zdo.IsValid() || ZDOMan.instance.GetZDO(zdo.m_uid) != zdo)
      reason = "Object no longer exists";
    else if (zdo.HasOwner() && !zdo.IsOwner() && ZNet.instance.GetPeer(zdo.GetOwner()) != null)
      reason = "Owned by another peer; retry during maintenance with players disconnected";
    else if (zdo.GetBool(ZDOVars.s_inUse) ||
      (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view) &&
        view.GetComponent<Container>() is Container container && container.IsInUse()))
      reason = "Chest is in use";
    return reason == "";
  }

  public static void Clear(ZDOData data)
  {
    data.Ints.Remove(ZDOVars.s_addedDefaultItems);
    data.Ints.Remove(ZDOVars.s_inUse);
    data.Strings.Remove(ZDOVars.s_items);
    data.ByteArrays.Remove(ZDOVars.s_items);
  }

  public static void Respawn(ZDO zdo)
  {
    ZDOData data = new(zdo);
    Clear(data);
    // Persistent receipt for server loot integrations; only actual resets set it.
    data.Ints["upgradeWorld•chestReset".GetStableHashCode()] = 1;
    var replacement = data.Clone();
    replacement.SetOwner(ZDOMan.GetSessionID());
    UpgradeWorld.Helper.RemoveZDO(zdo);
  }
}
