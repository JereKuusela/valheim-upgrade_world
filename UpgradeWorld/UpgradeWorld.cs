using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace UpgradeWorld;

[BepInPlugin(GUID, NAME, VERSION)]
public class UpgradeWorld : BaseUnityPlugin
{
  const string GUID = "upgrade_world";
  const string NAME = "Upgrade World";
  const string VERSION = "1.75";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public void Awake()
  {
    Log = Logger;
    Settings.Init(Config);
    new Harmony(GUID).PatchAll();
    SetupWatcher();
  }
  public void Start()
  {
    Executor.SetContext(this);
    CommandWrapper.Init();
    FiltererParameters.Parameters.Sort();
  }

  private void SetupWatcher()
  {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(Config.ConfigFilePath), Path.GetFileName(Config.ConfigFilePath));
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  bool WasConsoleVisible = false;
  public void LateUpdate()
  {
    if (!ZNet.instance)
    {
      LocationOperation.SetServerIds(null);
      VegetationOperation.SetServerIds(null);
      return;
    }
    var isConsoleVisible = Console.IsVisible();
    if (isConsoleVisible && !WasConsoleVisible)
      ServerExecution.RequestSync();

    WasConsoleVisible = isConsoleVisible;
  }

#pragma warning disable IDE0051 // Remove unused private members
  private void OnDestroy()
  {
#pragma warning restore IDE0051
    Executor.StopExecution();
    Config.Save();
  }
  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    }
    catch
    {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }

}

[HarmonyPatch(typeof(Console), nameof(Console.IsConsoleEnabled))]
public class IsConsoleEnabled
{
  static void Postfix(ref bool __result)
  {
    __result = true;
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  public static void Postfix()
  {
    new TimeChangeCommand();
    new BiomesCountCommand();
    new ObjectsCountCommand();
    new ZonesResetCommand();
    new ZonesGenerateCommand();
    new ObjectsListCommand();
    new LocationsListCommand();
    new LocationsAddCommand();
    new LocationsResetCommand();
    new ObjectsRemoveCommand();
    new ObjectsEditCommand();
    new ChestsSearchCommand();
    new LocationRegisterCommand();
    new LocationsRemoveCommand();
    new ChestsResetCommand();
    new ObjectsSwapCommand();
    new TimeSetCommand();
    new ObjetsRefreshCommand();
    new StartStopCommand();
    new UpgradeCommand();
    new VerboseCommand();
    new VegetationResetCommand();
    new VegetationAddCommand();
    new VegetationRemoveCommand();
    new SavingCommands();
    new BackupCommand();
    new WorldResetCommand();
    new ZonesRestoreCommand();
    new WorldVersionCommand();
    new WorldCleanCommand();
    new LocationsSwapCommand();
    new TempleVersionCommand();
    new CleanChestsCommand();
    new CleanDungeonsCommand();
    new CleanObjectsCommand();
    new CleanLocationsCommand();
    new CleanHealthCommand();
    new CleanSpawnsCommand();
    new CleanStandsCommand();
    new CleanDuplicatesCommand();
    new LocationsFixCommand();
    new LocationUnregisterCommand();
    new LocationsCountCommand();
    if (Terminal.commands.TryGetValue("genloc", out var genloc))
    {
      genloc.IsCheat = false;
      genloc.OnlyServer = false;
    }
  }
}

// Prevent crashes when too many ZDOs are being removed at once.
[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.SendDestroyed))]
public class SendDestroyed
{
  public static bool Prefix(ZDOMan __instance)
  {
    var list = __instance.m_destroySendList;
    if (list.Count < 10000)
      return true;
    ZPackage pkg = new();
    pkg.Write(10000);
    for (int i = 0; i < 10000; ++i)
      pkg.Write(list[i]);
    list.RemoveRange(0, 10000);
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DestroyZDO", pkg);
    return false;
  }
}