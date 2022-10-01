using HarmonyLib;

namespace UpgradeWorld;

// Location generation only places them on ungenerated zones. Skipping this check allows upgrading existing zones.
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.IsZoneGenerated))]
public class IsZoneGenerated {
  static bool Prefix(ref bool __result) {
    if (DistributeLocations.PlaceToAlreadyGenerated) {
      __result = false;
      return false;
    }
    return true;
  }
}

