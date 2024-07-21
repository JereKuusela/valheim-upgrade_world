
namespace UpgradeWorld;
/// <summary>Removes missing objects from armor and item stands.</summary>
public class CleanStands : EntityOperation
{
  public CleanStands(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var removed = 0;
    foreach (var zdo in zdos)
    {
      var r = removed;
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
      if (removed > r)
        AddPin(zdo.m_position);
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from armor stands");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (Clean(zdo, ""))
      {
        AddPin(zdo.m_position);
        removed++;
      }
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from item stands");
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
}
