using System;
using HarmonyLib;
namespace UpgradeWorld;
public class BackupCommand
{
  public static string Timestamp = "";
  public BackupCommand()
  {
    CommandWrapper.RegisterEmpty("backup");
    Helper.Command("backup", "- Saves the world with a timestamped filename.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
      Helper.Print(args.Context, $"Backing up to {ZNet.m_world.GetDBPath()}");
      ZNet.instance.Save(false);
    });
  }
}

[HarmonyPatch(typeof(World), nameof(World.GetDBPath), typeof(FileHelpers.FileSource))]
public class BackupDb
{
  static bool Prefix(World __instance, ref string __result)
  {
    if (BackupCommand.Timestamp != "")
    {
      __result = $"{World.GetWorldSavePath(FileHelpers.FileSource.Local)}/{__instance.m_fileName}_{BackupCommand.Timestamp}.db";
      return false;
    }
    return true;
  }
}
[HarmonyPatch(typeof(World), nameof(World.GetMetaPath), typeof(FileHelpers.FileSource))]
public class BackupMeta
{
  static bool Prefix(World __instance, ref string __result)
  {
    if (BackupCommand.Timestamp != "")
    {
      __result = $"{World.GetWorldSavePath(FileHelpers.FileSource.Local)}/{__instance.m_fileName}_{BackupCommand.Timestamp}.fwl";
      return false;
    }
    return true;
  }
}


[HarmonyPatch(typeof(ZNet), nameof(ZNet.SaveWorldThread))]
public class BackUpEnd
{
  static void Postfix()
  {
    BackupCommand.Timestamp = "";
  }
}