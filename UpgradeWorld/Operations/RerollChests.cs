using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Rerolls given chests.</summary>
  public class RerollChests : BaseOperation {
    public static List<string> ChestsNames = new List<string>() {
      "TreasureChest_blackforest", "TreasureChest_fCrypt", "TreasureChest_forestcrypt", "TreasureChest_heath",
      "TreasureChest_meadows", "TreasureChest_meadows_buried","TreasureChest_mountains", "TreasureChest_plains_stone",
      "TreasureChest_sunkencrypt", "TreasureChest_swamp", "TreasureChest_trollcave", "shipwreck_karve_chest",
      "loot_chest_wood", "loot_chest_stone" }.OrderBy(item => item).ToList();
    private string Id;
    private HashSet<string> AllowedItems;
    private int TotalChests = 0;
    private int RolledChests = 0;
    public RerollChests(string id, IEnumerable<string> allowedItems, Terminal context) : base(context) {
      Operation = "Reroll chests";
      AllowedItems = allowedItems.Select(Helper.Normalize).ToHashSet();
      Id = id;
    }

    protected override bool OnExecute() {
      var prefab = ZNetScene.instance.GetPrefab(Id);
      if (prefab == null || prefab.GetComponent<Container>() == null) {
        Failed = 1;
        return true;
      }
      var zdos = new List<ZDO>();
      ZDOMan.instance.GetAllZDOsWithPrefab(Id, zdos);
      foreach (var zdo in zdos) {
        TotalChests++;
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
        RolledChests++;
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
      return true;
    }
    private bool IsValid(ItemDrop.ItemData item) {
      var isValid = AllowedItems.Contains(Helper.Normalize(item.m_dropPrefab.name));
      if (Settings.Verbose && !isValid)
        Print("Skipping a chest: Extra item " + item.m_dropPrefab.name + ".");
      return isValid;
    }
    protected override void OnEnd() {
      if (Failed == 1)
        Print("Error: Invalid chest ID.");
      else if (Settings.Verbose)
        Print("Chests rerolled (" + RolledChests + " of " + TotalChests + ").");
      else
        Print("Chests rerolled.");
    }
  }
}