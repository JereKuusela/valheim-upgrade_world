
using Service;

namespace UpgradeWorld;
/// <summary>Removes missing objects from chests.</summary>
public class CleanChests : EntityOperation
{
  public CleanChests(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var removed = 0;
    foreach (var zdo in zdos)
    {
      var items = zdo.GetByteArray(ZDOVars.s_items);
      if (items.Length == 0) continue;
      ZPackage loadPackage = new(items);
      ZPackage savePackage = new();
      var result = CleanChest(loadPackage, savePackage);
      if (result == 0) continue;
      AddPin(zdo.m_position);
      removed += result;
      zdo.Set(ZDOVars.s_items, savePackage.GetArray());
      zdo.DataRevision += 100;
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from chests");
  }

  private int CleanChest(ZPackage from, ZPackage to)
  {
    var version = (Version.Item)from.ReadInt();
    from.SetPos(0);
    // Item Drawers mod uses the same ZDO key.
    // But luckily it writes 0 as version, so it can be detected.
    if (version == 0) return 0;
    var records = ItemDataHelper.Load(from);
    var removed = ItemDataHelper.CountInvalid(records);
    if (removed > 0)
      ItemDataHelper.Save(ItemDataHelper.RemoveInvalid(records), to);
    return removed;
  }

}

