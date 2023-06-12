using HarmonyLib;

namespace UpgradeWorld;

// Location generation only spawns them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.IsZoneGenerated))]
public class IsZoneGenerated {
  static bool Prefix(ref bool __result) {
    if (DistributeLocations.SpawnToAlreadyGenerated) {
      __result = false;
      return false;
    }
    return true;
  }
}

