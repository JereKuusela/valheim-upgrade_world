using System;
using HarmonyLib;
namespace UpgradeWorld;
public class BackupCommand {
  public static string Timestamp = "";
  public BackupCommand() {
    CommandWrapper.RegisterEmpty("backup");
    new Terminal.ConsoleCommand("backup", "- Saves the world with a timestamped filename.", (Terminal.ConsoleEventArgs args) => {
      if (Helper.IsClient(args)) return;
      Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
      Helper.Print(args.Context, "Saving..");
      ZNet.instance.Save(false);
    });
  }
}

[HarmonyPatch(typeof(World), nameof(World.GetDBPath))]
public class Backup {
  static bool Prefix(World __instance, ref string __result) {
    if (BackupCommand.Timestamp != "") {
      __result = $"{__instance.m_worldSavePath}/{__instance.m_name}_{BackupCommand.Timestamp}.db";
      BackupCommand.Timestamp = "";
      return false;
    }
    return true;
  }
}
