using HarmonyLib;
namespace UpgradeWorld;
public class SavingCommands
{
  public static bool SavingDisabled = false;
  public SavingCommands()
  {
    CommandWrapper.RegisterEmpty("save_disable");
    Helper.Command("save_disable", "- Disables saving.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      SavingDisabled = true;
      Helper.Print(args.Context, "Saving disabled.");
    });
    CommandWrapper.RegisterEmpty("save_enable");
    Helper.Command("save_enable", "- Enables saving.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      SavingDisabled = false;
      ZoneSystem.instance.m_didZoneTest = false;
      Helper.Print(args.Context, "Saving enabled.");
    });
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SkipSaving))]
public class SkipSaving
{
  static bool Prefix(ref bool __result)
  {
    if (SavingCommands.SavingDisabled)
    {
      __result = true;
      return false;
    }
    return true;
  }
}
