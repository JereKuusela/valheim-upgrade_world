using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
namespace UpgradeWorld;
[BepInPlugin(GUID, NAME, VERSION)]
public class UpgradeWorld : BaseUnityPlugin {
  const string GUID = "upgrade_world";
  const string NAME = "Upgrade World";
  const string VERSION = "1.15";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public void Awake() {
    Log = Logger;
    Settings.Init(Config);
    Harmony harmony = new(GUID);
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
  public static void Postfix(Terminal __instance) {
    new TimeChangeCommand();
    new BiomesCountCommand();
    new ObjectsCountCommand();
    new ZonesResetCommand();
    new ZonesGenerateCommand();
    new ObjectsListCommand();
    new LocationsAddCommand();
    new LocationsResetCommand();
    new ObjectsRemoveCommand();
    new LocationsRemoveCommand();
    new ChestsResetCommand();
    new TimeSetCommand();
    new VegetationSetCommands();
    new StartStopCommand();
    new UpgradeCommand();
    new VerboseCommand();
    new VegetationResetCommand();
    new VegetationAddCommand();
    new SavingCommands();
    new BackupCommand();
    new ZonesRestoreCommand();
    //new VegetationExportCommand();
    if (Terminal.commands.TryGetValue("genloc", out var genloc)) {
      genloc.IsCheat = false;
      genloc.OnlyServer = false;
    }
  }
}