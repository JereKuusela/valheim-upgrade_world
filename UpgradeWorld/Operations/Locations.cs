using HarmonyLib;
using System;
using System.Collections.Generic;

namespace UpgradeWorld {

  public partial class Operation {
    public static bool PlaceToAlreadyGenerated = false;
    /// <summary>Re-places whitelisted locations to also already generated zones</summary>
    public static void RedistributeLocations() {
      PlaceToAlreadyGenerated = true;
      ZoneSystem.instance.GenerateLocations();
      PlaceToAlreadyGenerated = false;
    }
  }
  // Location generation only places them on ungenerated zones. Skipping this check allows upgrading existing zones.
  [HarmonyPatch(typeof(ZoneSystem), "IsZoneGenerated")]
  public class IsZoneGenerated {
    public static bool Prefix(ref bool __result) {
      if (Operation.PlaceToAlreadyGenerated) {
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
      if (Operation.PlaceToAlreadyGenerated) {
        var dictionary = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();
        foreach (var keyValuePair in __instance.m_locationInstances) {
          if (keyValuePair.Value.m_placed || !Settings.IsLocationIncluded(keyValuePair.Value.m_location.m_prefabName)) {
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
      return !Operation.PlaceToAlreadyGenerated || Settings.IsLocationIncluded(location.m_prefabName);
    }
  }
}