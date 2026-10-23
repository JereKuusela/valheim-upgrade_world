namespace UpgradeWorld;
/// <summary>Removes persistent objects and events from zones that are not generated (leftovers that regenerate as duplicates).</summary>
public class PruneSaveSystem : EntityOperation
{
  public PruneSaveSystem(Terminal context, ZDO[] zdos, FiltererParameters args) : base(context, args.Pin)
  {
    Clean(zdos, args);
  }

  private void Clean(ZDO[] zdos, FiltererParameters args)
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
    // Events can be in non-generated zones, so have to get rid of them too.
    var events = PersistentEvents.Active().FindAll(e => !zs.m_generatedZones.Contains(ZoneSystem.GetZone(e.position)) && args.FilterPosition(e.position, false));
    foreach (var e in events)
    {
      AddPin(e.position);
      if (Settings.Verbose)
        Print($"Removed persistent event {e.internalName} at {Helper.PrintVectorXZY(e.position)}.");
    }
    var eventObjects = PersistentEvents.Remove(events);
    Print($"Removed {removed} object{S(removed)} from non-generated zones.");
    Print($"Removed {events.Count} persistent event{S(events.Count)} from non-generated zones ({eventObjects} object{S(eventObjects)}).");
  }
}
