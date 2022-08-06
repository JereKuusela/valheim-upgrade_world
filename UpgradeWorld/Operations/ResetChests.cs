using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Rerolls given chests.</summary>
public class ResetChests : EntityOperation {
  public static List<string> ChestsNames = new List<string> {
      "TreasureChest_blackforest", "TreasureChest_fCrypt", "TreasureChest_forestcrypt", "TreasureChest_heath",
      "TreasureChest_meadows", "TreasureChest_meadows_buried","TreasureChest_mountains", "TreasureChest_plains_stone",
      "TreasureChest_sunkencrypt", "TreasureChest_swamp", "TreasureChest_trollcave", "shipwreck_karve_chest",
      "loot_chest_wood", "loot_chest_stone", "*" }.OrderBy(item => item).ToList();
  private HashSet<string> AllowedItems;
  public ResetChests(string chestId, IEnumerable<string> allowedItems, bool looted, DataParameters args, Terminal context) : base(context) {
    AllowedItems = allowedItems.Select(Helper.Normalize).ToHashSet();
    Reroll(chestId, looted, args);
  }

  private void Reroll(string chestId, bool looted, DataParameters args) {
    var chestIds = chestId == "*" ? ChestsNames.Where(name => name != "*") : new List<string> { chestId };
    var totalChests = 0;
    var resetedChests = 0;
    var prefabs = chestIds.ToDictionary(id => id.GetStableHashCode(), id => ZNetScene.instance.GetPrefab(id));
    if (prefabs.Values.Any(prefab => prefab == null || prefab.GetComponent<Container>() == null)) {
      Print("Error: Invalid chest ID.");
      return;
    }
    var zdos = chestIds.Select(name => GetZDOs(name, args)).Aggregate(new List<ZDO>(), (acc, value) => { acc.AddRange(value); return acc; });
    foreach (var zdo in zdos) {
      if (!args.Roll()) {
        if (Settings.Verbose)
          Print("Skipping a chest: Random roll.");
        continue;
      }
      totalChests++;
      if (!zdo.GetBool(Hash.AddedDefaultItems, false)) {
        if (Settings.Verbose)
          Print("Skipping a chest: Drops already unrolled.");
        continue;
      }
      var obj = ZNetScene.instance.FindInstance(zdo);
      Inventory inventory;
      if (obj) {
        inventory = obj.GetComponent<Container>().GetInventory();
      } else {
        var container = prefabs[zdo.GetPrefab()].GetComponent<Container>();
        inventory = new(container.m_name, container.m_bkg, container.m_width, container.m_height);
        ZPackage loadPackage = new(zdo.GetString(Hash.Items, ""));
        inventory.Load(loadPackage);
      }
      if (inventory.GetAllItems().Count == 0 && !looted) {
        if (Settings.Verbose)
          Print("Skipping a chest: Already looted.");
        continue;
      }
      if (AllowedItems.Count > 0 && !inventory.GetAllItems().All(IsValid)) continue;
      resetedChests++;
      inventory.RemoveAll();
      if (obj) {
        obj.GetComponent<Container>().AddDefaultItems();
      } else {
        zdo.Set(Hash.AddedDefaultItems, false);
        ZPackage savePackage = new();
        inventory.Save(savePackage);
        zdo.Set(Hash.Items, savePackage.GetBase64());
      }
    }
    if (Settings.Verbose)
      Print("Chests reseted (" + resetedChests + " of " + totalChests + ").");
    else
      Print("Chests reseted.");
  }
  private bool IsValid(ItemDrop.ItemData item) {
    var isValid = AllowedItems.Contains(Helper.Normalize(item.m_dropPrefab.name));
    if (Settings.Verbose && !isValid)
      Print("Skipping a chest: Extra item " + item.m_dropPrefab.name + ".");
    return isValid;
  }
}
