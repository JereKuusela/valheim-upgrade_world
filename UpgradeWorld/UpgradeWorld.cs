using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
namespace UpgradeWorld;
[BepInPlugin("valheim.jere.upgrade_world", "UpgradeWorld", "1.11.0.0")]
public class UpgradeWorld : BaseUnityPlugin {
  public static ManualLogSource Log;
  public void Awake() {
    Log = Logger;
    Settings.Init(Config);
    Harmony harmony = new("valheim.jere.upgrade_world");
    harmony.PatchAll();
  }
  public void Start() {
    CommandWrapper.Init();
    FiltererParameters.Parameters.Sort();
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
[HarmonyPatch(typeof(Terminal), "InitTerminal")]
public class SetCommands {
  public static void Postfix() {
    new ChangeDayCommand();
    new ChangeTimeCommand();
    new CountBiomesCommand();
    new CountEntitiesCommand();
    new DestroyCommand();
    new DistributeCommand();
    new GenerateCommand();
    new ListEntitiesCommand();
    new PlaceLocationsCommand();
    new RegenerateLocationsCommand();
    new RemoveEntitiesCommand();
    new RemoveLocationsCommand();
    new RerollChestsCommand();
    new SetDayCommand();
    new SetTimeCommand();
    new SetVegetationCommand();
    new StartStopCommand();
    new UpgradeCommand();
    new VerboseCommand();
  }
}