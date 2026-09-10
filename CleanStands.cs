using Service;

namespace UpgradeWorld;
/// <summary>Removes missing objects from armor and item stands.</summary>
public class CleanStands : EntityOperation
{
  public CleanStands(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    var removed = 0;
    var skipped = 0;
    foreach (var zdo in zdos)
    {
      var changed = false;
      foreach (var prefix in StandItems.Prefixes(zdo))
      {
        var hash = StandItems.GetHash(zdo, prefix);
        if (hash == 0 || ZNetScene.instance.GetPrefab(hash) != null) continue;
        if (!ChestInventory.CanModify(zdo, out var reason))
        {
          skipped++;
          if (Settings.Verbose) Print($"Skipping stand {zdo.m_uid}: {reason}");
          break;
        }
        if (!zdo.IsOwner()) zdo.SetOwner(ZDOMan.GetSessionID());
        zdo.Set(prefix + "item", 0);
        zdo.Set(prefix + "item", "");
        zdo.Set(prefix + "variant", 0);
        if (prefix == "")
        {
          zdo.Set(prefix + "quality", 1);
          zdo.Set(ZDOVars.s_type, 0);
        }
        removed++;
        changed = true;
      }
      if (changed) AddPin(zdo.GetPosition());
    }
    if (alwaysPrint || removed > 0 || skipped > 0)
      Print($"Removed {removed} missing items from stands; skipped {skipped} unsafe stands (unchanged).");
  }
}
