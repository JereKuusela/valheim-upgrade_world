using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Rerolls given chests.</summary>
  public class RerollChests : EntityOperation {
    public static List<string> ChestsNames = new List<string>() {
      "TreasureChest_blackforest", "TreasureChest_fCrypt", "TreasureChest_forestcrypt", "TreasureChest_heath",
      "TreasureChest_meadows", "TreasureChest_meadows_buried","TreasureChest_mountains", "TreasureChest_plains_stone",
      "TreasureChest_sunkencrypt", "TreasureChest_swamp", "TreasureChest_trollcave", "shipwreck_karve_chest",
      "loot_chest_wood", "loot_chest_stone" }.OrderBy(item => item).ToList();
    private HashSet<string> AllowedItems;
    public RerollChests(string id, IEnumerable<string> allowedItems, FiltererParameters args, Terminal context) : base(context) {
      AllowedItems = allowedItems.Select(Helper.Normalize).ToHashSet();
      Reroll(id, args);
    }

    private void Reroll(string id, FiltererParameters args) {
      var totalChests = 0;
      var rolledChests = 0; ;
      var prefab = ZNetScene.instance.GetPrefab(id);
      if (prefab == null || prefab.GetComponent<Container>() == null) {
        Print("Error: Invalid chest ID.");
        return;
      }
      var zdos = GetZDOs(id, args);
      foreach (var zdo in zdos) {
        totalChests++;
        if (!zdo.GetBool("addedDefaultItems", false)) {
          if (Settings.Verbose)
            Print("Skipping a chest: Drops already unrolled.");
          continue;
        }
        var obj = ZNetScene.instance.FindInstance(zdo);
        Inventory inventory;
        if (obj) {
          inventory = obj.GetComponent<Container>().GetInventory();
        } else {
          var container = prefab.GetComponent<Container>();
          inventory = new Inventory(container.m_name, container.m_bkg, container.m_width, container.m_height);
          var loadPackage = new ZPackage(zdo.GetString("items", ""));
          inventory.Load(loadPackage);
        }
        if (inventory.GetAllItems().Count == 0) {
          if (Settings.Verbose)
            Print("Skipping a chest: Already looted.");
          continue;
        }
        if (!inventory.GetAllItems().All(IsValid)) continue;
        rolledChests++;
        inventory.RemoveAll();
        if (obj) {
          obj.GetComponent<Container>().AddDefaultItems();
        } else {
          zdo.Set("addedDefaultItems", false);
          var savePackage = new ZPackage();
          inventory.Save(savePackage);
          zdo.Set("items", savePackage.GetBase64());
        }
      }
      if (Settings.Verbose)
        Print("Chests rerolled (" + rolledChests + " of " + totalChests + ").");
      else
        Print("Chests rerolled.");
    }
    private bool IsValid(ItemDrop.ItemData item) {
      var isValid = AllowedItems.Contains(Helper.Normalize(item.m_dropPrefab.name));
      if (Settings.Verbose && !isValid)
        Print("Skipping a chest: Extra item " + item.m_dropPrefab.name + ".");
      return isValid;
    }
  }
}