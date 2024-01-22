using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Rerolls given chests.</summary>
public class ResetChests : EntityOperation
{
  private static List<string> chestNames = [];
  private readonly HashSet<string> AllowedItems;
  public ResetChests(string[] chestIds, IEnumerable<string> allowedItems, bool looted, DataParameters args, Terminal context) : base(context)
  {
    AllowedItems = allowedItems.Select(Helper.Normalize).ToHashSet();
    Reroll(chestIds, looted, args);
  }
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
  private void Reroll(string[] chestIds, bool looted, DataParameters args)
  {
    var totalChests = 0;
    var resetedChests = 0;
    var prefabs = chestIds.ToDictionary(id => id.GetStableHashCode(), id => ZNetScene.instance.GetPrefab(id));
    if (prefabs.Values.Any(prefab => prefab == null || prefab.GetComponent<Container>() == null))
    {
      Print("Error: Invalid chest ID.");
      return;
    }
    var zdos = GetZDOs(args, [.. prefabs.Keys]);
    foreach (var zdo in zdos)
    {
      if (!args.Roll())
      {
        if (Settings.Verbose)
          Print("Skipping a chest: Random roll.");
        continue;
      }
      totalChests++;
      if (!zdo.GetBool(ZDOVars.s_addedDefaultItems))
      {
        if (Settings.Verbose)
          Print("Skipping a chest: Drops already unrolled.");
        continue;
      }
      if (!looted || AllowedItems.Count > 0)
      {
        var container = prefabs[zdo.m_prefab].GetComponent<Container>();
        Inventory inventory = new(container.m_name, container.m_bkg, container.m_width, container.m_height);
        ZPackage loadPackage = new(zdo.GetString(ZDOVars.s_items));
        inventory.Load(loadPackage);
        if (inventory.GetAllItems().Count == 0 && !looted)
        {
          if (Settings.Verbose)
            Print("Skipping a chest: Already looted.");
          continue;
        }
        if (AllowedItems.Count > 0 && !inventory.GetAllItems().All(IsValid)) continue;
      }

      ZDOData data = new(zdo);
      data.Ints.Remove(ZDOVars.s_addedDefaultItems);
      data.Strings.Remove(ZDOVars.s_items);
      data.Clone();
      resetedChests++;
      AddPin(zdo.m_position);
      Helper.RemoveZDO(zdo);

      ResetTerrain.Execute(zdo.GetPosition(), args.TerrainReset);
    }
    Print("Chests reseted (" + resetedChests + " of " + totalChests + ").");
    PrintPins();
  }
  private bool IsValid(ItemDrop.ItemData item)
  {
    var isValid = AllowedItems.Contains(Helper.Normalize(item.m_dropPrefab.name));
    if (Settings.Verbose && !isValid)
      Print("Skipping a chest: Extra item " + item.m_dropPrefab.name + ".");
    return isValid;
  }
}
