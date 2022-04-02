using BepInEx;
using HarmonyLib;
namespace UpgradeWorld;
[BepInPlugin("valheim.jere.upgrade_world", "UpgradeWorld", "1.11.0.0")]
public class UpgradeWorld : BaseUnityPlugin {

  public void Awake() {
    Settings.Init(Config);
    Harmony harmony = new("valheim.jere.upgrade_world");
    harmony.PatchAll();
    Commands.Init();
  }
  public void Update() {
    Executor.Execute();
  }
}

[HarmonyPatch(typeof(Console), nameof(Console.IsConsoleEnabled))]
public class IsConsoleEnabled {
  static void Postfix(ref bool __result) {
    __result = true;
  }
}
[HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
public class PreventDoubleZNetView {
  static bool Prefix(ZNetView __instance) {
    if (!Settings.PreventDoubleZNetView) return true;
    if (ZNetView.m_forceDisableInit || ZDOMan.instance == null) return true;
    if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null) {
      ZLog.LogWarning($"Preventing double ZNetView for {__instance.gameObject.name}. Use 'remove_entities' command to remove these objects.");
      UnityEngine.Object.Destroy(__instance);
      return false;
    }
    return true;
  }
}
