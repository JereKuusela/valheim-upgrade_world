using HarmonyLib;

namespace UpgradeWorld;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Load))]
public class PreventGenloc
{
  static void Postfix(ZoneSystem __instance)
  {
    if (Settings.DisableAutomaticGenloc && !__instance.m_locationsGenerated && __instance.m_locationInstances.Count > 0)
    {
      __instance.m_locationsGenerated = true;
      UpgradeWorld.Log.LogWarning("Skipped automatic genloc. Run the command manually if needed.");
    }
  }
}

