using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Removes missing item records without rewriting surviving items.</summary>
public class CleanChests : EntityOperation
{
  public CleanChests(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    var removed = 0;
    var skipped = 0;
    foreach (var zdo in zdos)
    {
      var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
      if (prefab == null || prefab.GetComponent<Container>() == null) continue;
      if (!ChestInventory.CanModify(zdo, out var reason))
      {
        skipped++;
        if (Settings.Verbose) Print($"Skipping chest {zdo.m_uid}: {reason}");
        continue;
      }
      if (!ChestInventory.TryRead(zdo, out var inventory, out var error))
      {
        skipped++;
        if (Settings.Verbose) Print($"Skipping chest {zdo.m_uid}: {error}");
        continue;
      }
      bool Exists(ChestInventory.Entry item) => ZNetScene.instance.GetPrefab(item.Hash) is UnityEngine.GameObject itemPrefab &&
        itemPrefab.GetComponent<ItemDrop>() != null;
      var missing = inventory.Items.Count(item => !Exists(item));
      if (missing == 0) continue;
      // Loaded inventories can save a stale in-memory copy over this edit.
      // Defer these until unloaded, instead of causing an ownership/load race.
      if (ZNetScene.instance.m_instances.ContainsKey(zdo))
      {
        skipped++;
        if (Settings.Verbose) Print($"Skipping loaded chest {zdo.m_uid}; retry after unloading the area.");
        continue;
      }
      var bytes = inventory.Keep(Exists); // prepare fully before touching the ZDO
      if (!zdo.IsOwner()) zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.Set(ZDOVars.s_items, bytes);
      zdo.Set(ZDOVars.s_items, "");
      AddPin(zdo.m_position);
      removed += missing;
    }
    if (alwaysPrint || removed > 0 || skipped > 0)
      Print($"Removed {removed} missing item records from chests. Skipped {skipped} unsafe/unreadable chests (unchanged; verbose for reasons).");
  }
}
