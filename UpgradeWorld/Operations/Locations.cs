using System;
using System.Collections.Generic;
using HarmonyLib;

namespace UpgradeWorld {

  public partial class Operations {
    /// <summary>Controls if only locations defined in the config get redistributed.</summary>
    public static bool OnlyConfigured = false;
    /// <summary>Controls if the redistribution can also target already generated zones.</summary>
    public static bool PlaceToAlreadyGenerated = false;
    /// <summary>Re-places whitelisted locations to also already generated zones</summary>
    public static void RedistributeLocations(bool onlyConfigured, bool toAlreadyGenerated) {
      OnlyConfigured = onlyConfigured;
      PlaceToAlreadyGenerated = toAlreadyGenerated;
      ZoneSystem.instance.GenerateLocations();
      PlaceToAlreadyGenerated = false;
      OnlyConfigured = false;
    }
    /// <summary>Re-places whitelisted locations to also already generated zones</summary>
    public static void RedistributeLocations2() {
      OnlyConfigured = true;
      ZoneSystem.instance.GenerateLocations();
      OnlyConfigured = false;
    }
  }
  // Location generation only places them on ungenerated zones. Skipping this check allows upgrading existing zones.
  [HarmonyPatch(typeof(ZoneSystem), "IsZoneGenerated")]
  public class IsZoneGenerated {
    public static bool Prefix(ref bool __result) {
      if (Operations.PlaceToAlreadyGenerated) {
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
      if (Operations.OnlyConfigured) {
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
      return !Operations.PlaceToAlreadyGenerated || Settings.IsLocationIncluded(location.m_prefabName);
    }
  }
}