using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace UpgradeWorld {

  // Location generation only places them on ungenerated zones. Skipping this check allows upgrading existing zones.
  [HarmonyPatch(typeof(ZoneSystem), "IsZoneGenerated")]
  public class IsZoneGenerated {
    public static bool Prefix(ref bool __result) {
      if (DistributeLocations.PlaceToAlreadyGenerated) {
        __result = false;
        return false;
      }
      return true;
    }
  }
  // Optimization to only remove placement from affected locations.
  [HarmonyPatch(typeof(ZoneSystem), "ClearNonPlacedLocations")]
  public class ClearNonPlacedLocations {
    public static bool Prefix(ZoneSystem __instance) {
      if (DistributeLocations.Ids.Count() > 0) {
        var dictionary = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();
        foreach (var keyValuePair in __instance.m_locationInstances) {
          if (keyValuePair.Value.m_placed || !DistributeLocations.Ids.Contains(keyValuePair.Value.m_location.m_prefabName.ToLower())) {
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
  [HarmonyPatch(typeof(ZoneSystem), "GenerateLocations", new Type[] { typeof(ZoneSystem.ZoneLocation) })]
  public class GenerateLocations {
    public static bool Prefix(ZoneSystem.ZoneLocation location) {
      return DistributeLocations.Ids.Count() == 0 || DistributeLocations.Ids.Contains(location.m_prefabName.ToLower());
    }
  }
}