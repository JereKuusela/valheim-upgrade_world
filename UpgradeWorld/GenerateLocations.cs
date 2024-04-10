using HarmonyLib;

namespace UpgradeWorld;

// Location generation only spawns them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.IsZoneGenerated))]
public class IsZoneGenerated
{
  static bool Prefix(Vector2i zoneID, ref bool __result)
  {
    if (DistributeLocations.SpawnToAlreadyGenerated)
    {
      __result = !DistributeLocations.AllowedZones.Contains(zoneID);
      return false;
    }
    return true;
  }
}


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

