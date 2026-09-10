using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Rerolls given chests.</summary>
public class ResetChests(string[] chestIds, IEnumerable<string> allowedItems, bool looted, DataParameters args, Terminal context) : ExecutedEntityOperation(context, chestIds, args)
{
  private static List<string> chestNames = [];
  private readonly HashSet<string> AllowedItems = [.. allowedItems.Select(Helper.Normalize)];
  private readonly string[] ChestIds = chestIds;
  private readonly bool Looted = looted;
  private Dictionary<int, UnityEngine.GameObject> ChestPrefabs = [];
  private readonly Dictionary<string, int> Skipped = [];

  protected override HashSet<int> GetPrefabsForOperation()
  {
    ChestPrefabs = ChestIds.ToDictionary(id => id.GetStableHashCode(), id => ZNetScene.instance.GetPrefab(id));
    if (ChestPrefabs.Values.Any(prefab => prefab == null || prefab.GetComponent<Container>() == null || prefab.GetComponent<Container>().m_defaultItems.IsEmpty()))
      throw new System.InvalidOperationException("Error: Chest ID must have a non-empty default loot table.");

    return [.. ChestPrefabs.Keys];
  }

  protected override bool ProcessZDO(ZDO zdo)
  {
    if (!ShouldResetChest(zdo)) return false;

    var position = zdo.GetPosition();
    ChestInventory.Respawn(zdo);

    ResetTerrain.Execute(position, Args.TerrainReset);
    return true;
  }

  protected override string GetNoObjectsMessage() => "No chests found to reset.";

  protected override string GetInitMessage() => $"Checking {TotalCount} chest candidates. " +
    (Looted ? "Includes empty and non-empty chests. " : "Empty chests are skipped. ") +
    (AllowedItems.Count == 0 ? "No item allowlist: player-stored items in these chest prefabs can be replaced." : "Skips chests containing items outside the allowlist.");

  protected override string GetProcessedMessage() => $"Chests reset ({ProcessedCount} of {TotalCount} candidates)." +
    string.Concat(Skipped.Select(pair => $"\nSkipped {pair.Value}: {pair.Key}."));

  protected override string GetCountMessage(int count, int prefab) => "";

  public static List<string> ChestNames()
  {
    if (chestNames.Count == 0)
    {
      chestNames = [.. ZNetScene.instance.m_prefabs
        .Where(prefab => prefab.TryGetComponent<Container>(out var container) && !container.m_defaultItems.IsEmpty())
        .Select(obj => obj.name).OrderBy(item => item)];
    }
    return chestNames;
  }

  private bool Skip(string reason)
  {
    Skipped[reason] = Skipped.TryGetValue(reason, out var count) ? count + 1 : 1;
    if (Settings.Verbose) Print("Skipping a chest: " + reason + ".");
    return false;
  }

  private bool ShouldResetChest(ZDO zdo)
  {
    if (!ChestInventory.CanModify(zdo, out var reason)) return Skip(reason);
    if (!zdo.GetBool(ZDOVars.s_addedDefaultItems)) return Skip("Loot already unrolled");
    // Always validate serialized contents, including with the looted flag.
    if (!ChestInventory.TryRead(zdo, out var inventory, out var error))
    {
      if (Settings.Verbose) Print($"Chest {zdo.m_uid}: {error}");
      return Skip("Unreadable or unsupported inventory (unchanged)");
    }
    if (inventory.Items.Count == 0 && !Looted) return Skip("Already looted");
    if (AllowedItems.Count > 0 && inventory.Items.Any(item => !IsAllowed(item)))
      return Skip("Item outside allowlist (possible player storage)");
    return true;
  }

  private bool IsAllowed(ChestInventory.Entry item)
  {
    var prefab = ZNetScene.instance.GetPrefab(item.Hash);
    // Unknown hashes must never disappear through Inventory.Load before this check.
    return prefab != null && AllowedItems.Contains(Helper.Normalize(prefab.name));
  }
}
