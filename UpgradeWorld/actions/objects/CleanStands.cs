
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
    string[] prefixes = ["0_", "1_", "2_", "3_", "4_", "5_", "6_", "7_", "8_", "9_", "10_", "11_", "12_", "13_", "14_", "15_"];
    foreach (var zdo in zdos)
    {
      var r = removed;
      foreach (var prefix in prefixes)
      {
        if (Clean(zdo, prefix))
          removed++;
      }
      if (removed > r)
      {
        AddPin(zdo.m_position);
        zdo.DataRevision += 100;
      }
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from armor stands");

    removed = 0;
    foreach (var zdo in zdos)
    {
      if (Clean(zdo, ""))
      {
        AddPin(zdo.m_position);
        zdo.DataRevision += 100;
        removed++;
      }
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)} from item stands");
  }

  private bool Clean(ZDO zdo, string prefix)
  {
    var zs = ZNetScene.instance;
    var item = zdo.GetInt(prefix + "item", 0);
    if (item == 0) return false;
    if (zs.m_namedPrefabs.ContainsKey(item)) return false;
    zdo.Set(prefix + "item", 0);
    zdo.Set(prefix + "variant", 0);
    if (prefix == "")
    {
      zdo.Set(prefix + "quality", 1);
      zdo.Set(ZDOVars.s_type, 0);
    }
    return true;
  }
}
