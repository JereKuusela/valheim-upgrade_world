
namespace UpgradeWorld;
/// <summary>Removes missing objects from chests.</summary>
public class CleanChests : EntityOperation
{
  public CleanChests(Terminal context, ZDO[] zdos, bool pin) : base(context, pin)
  {
    Clean(zdos);
  }

  private void Clean(ZDO[] zdos)
  {
    var removed = 0;
    foreach (var zdo in zdos)
    {
      var items = zdo.GetString(ZDOVars.s_items);
      if (items == "") continue;
      ZPackage loadPackage = new(items);
      ZPackage savePackage = new();
      var result = CleanChest(loadPackage, savePackage);
      if (result == 0) continue;
      AddPin(zdo.m_position);
      removed += result;
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.Set(ZDOVars.s_items, savePackage.GetBase64());
    }
    if (removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from chests");
  }

  private int CleanChest(ZPackage from, ZPackage to)
  {
    int version = from.ReadInt();
    // Item Drawers mod uses the same ZDO key.
    // But luckily it writes 0 as version, so it can be detected.
    if (version == 0) return 0;
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
