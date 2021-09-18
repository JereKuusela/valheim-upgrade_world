using BepInEx;
using HarmonyLib;

namespace UpgradeWorld {
  [BepInPlugin("valheim.jere.upgrade_world", "UpgradeWorld", "0.1.0.0")]
  public class UpgradeWorld : BaseUnityPlugin {

    public void Awake() {
      Settings.Init(Config);
      var harmony = new Harmony("valheim.jere.item_stand_all_items");
      harmony.PatchAll();
    }
    public void Update() {
      Operation.Process();
    }
  }
}
