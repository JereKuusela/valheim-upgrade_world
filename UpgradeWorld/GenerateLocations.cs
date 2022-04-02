using System.Collections.Generic;
using System.Linq;
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
// Optimization to only remove placement from affected locations.
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ClearNonPlacedLocations))]
public class ClearNonPlacedLocations {
  static bool Prefix(ZoneSystem __instance) {
    if (DistributeLocations.DistributedIds.Count() > 0) {
      Dictionary<Vector2i, ZoneSystem.LocationInstance> dictionary = new();
      foreach (var keyValuePair in __instance.m_locationInstances) {
        if (keyValuePair.Value.m_placed || !DistributeLocations.DistributedIds.Contains(keyValuePair.Value.m_location.m_prefabName.ToLower())) {
          dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
      }
      __instance.m_locationInstances = dictionary;
      return false;
    }
    return true;
  }
}
// Optimization to only run generation code for affected locations.
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocations {
  static bool Prefix(ZoneSystem.ZoneLocation location) {
    return DistributeLocations.DistributedIds.Count() == 0 || DistributeLocations.DistributedIds.Contains(location.m_prefabName.ToLower());
  }
}
