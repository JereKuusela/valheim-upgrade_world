using BepInEx;
using HarmonyLib;

namespace UpgradeWorld {
  [BepInPlugin("valheim.jere.upgrade_world", "UpgradeWorld", "1.6.0.0")]
  public class UpgradeWorld : BaseUnityPlugin {

    public void Awake() {
      Settings.Init(Config);
      var harmony = new Harmony("valheim.jere.upgrade_world");
      harmony.PatchAll();
      Commands.Init();
    }
    public void Update() {
      Executor.Execute();
    }
  }

  [HarmonyPatch(typeof(Console), "IsConsoleEnabled")]
  public class IsConsoleEnabled {
    public static void Postfix(ref bool __result) {
      __result = true;
    }
  }
}
