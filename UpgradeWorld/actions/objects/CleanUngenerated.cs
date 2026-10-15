namespace UpgradeWorld;
/// <summary>Removes persistent objects from zones that are not generated (leftovers that regenerate as duplicates).</summary>
public class CleanUngenerated : EntityOperation
{
  public CleanUngenerated(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var zs = ZoneSystem.instance;
    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (!zdo.Persistent || zdo.OutsideZones) continue;
      if (zs.m_generatedZones.Contains(ZoneSystem.GetZone(zdo.m_position))) continue;
      AddPin(zdo.m_position);
      Helper.RemoveZDO(zdo);
      removed++;
      if (Settings.Verbose)
        Print($"Removed object {zdo.m_prefab} at {zdo.m_position} from a non-generated zone.");
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} object{S(removed)} from non-generated zones.");
  }
}
