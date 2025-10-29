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

  protected override HashSet<int> GetPrefabsForOperation()
  {
    ChestPrefabs = ChestIds.ToDictionary(id => id.GetStableHashCode(), id => ZNetScene.instance.GetPrefab(id));
    if (ChestPrefabs.Values.Any(prefab => prefab == null || prefab.GetComponent<Container>() == null))
      throw new System.InvalidOperationException("Error: Invalid chest ID.");

    return [.. ChestPrefabs.Keys];
  }

  protected override bool ProcessZDO(ZDO zdo)
  {
    if (!ShouldResetChest(zdo)) return false;

    ZDOData data = new(zdo);
    data.Ints.Remove(ZDOVars.s_addedDefaultItems);
    data.Strings.Remove(ZDOVars.s_items);
    data.Clone();
    Helper.RemoveZDO(zdo);

    ResetTerrain.Execute(zdo.GetPosition(), Args.TerrainReset);
    return true;
  }

  protected override string GetNoObjectsMessage() => "No chests found to reset.";

  protected override string GetInitMessage() => $"Resetting {TotalCount} chest{(TotalCount > 1 ? "s" : "")}";

  protected override string GetProcessedMessage() => $"Chests reseted ({ProcessedCount} of {TotalCount}).";

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

  private bool ShouldResetChest(ZDO zdo)
  {
    if (!zdo.GetBool(ZDOVars.s_addedDefaultItems))
    {
      if (Settings.Verbose)
        Print("Skipping a chest: Drops already unrolled.");
      return false;
    }

    if (!Looted || AllowedItems.Count > 0)
    {
      var container = ChestPrefabs[zdo.m_prefab].GetComponent<Container>();
      Inventory inventory = new(container.m_name, container.m_bkg, container.m_width, container.m_height);
      ZPackage loadPackage = new(zdo.GetString(ZDOVars.s_items));
      inventory.Load(loadPackage);

      if (inventory.GetAllItems().Count == 0 && !Looted)
      {
        if (Settings.Verbose)
          Print("Skipping a chest: Already looted.");
        return false;
      }

      if (AllowedItems.Count > 0 && !inventory.GetAllItems().All(IsValid))
        return false;
    }

    return true;
  }
  private bool IsValid(ItemDrop.ItemData item)
  {
    var isValid = AllowedItems.Contains(Helper.Normalize(item.m_dropPrefab.name));
    if (Settings.Verbose && !isValid)
      Print("Skipping a chest: Extra item " + item.m_dropPrefab.name + ".");
    return isValid;
  }
}
