using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace UpgradeWorld {

  public partial class Operations {
    /// <summary>Controls if only locations defined in the config get redistributed.</summary>
    public static string[] IncludedIds = new string[0];
    /// <summary>Controls if the redistribution can also target already generated zones.</summary>
    public static bool PlaceToAlreadyGenerated = false;
    /// <summary>Re-places whitelisted locations to also already generated zones</summary>
    public static void RedistributeLocations(string[] ids, bool toAlreadyGenerated) {
      IncludedIds = ids.Select(item => item.ToLower()).ToArray();
      PlaceToAlreadyGenerated = toAlreadyGenerated;
      ZoneSystem.instance.GenerateLocations();
      PlaceToAlreadyGenerated = false;
      IncludedIds = new string[0];
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
      if (Operations.IncludedIds.Length > 0) {
        var dictionary = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();
        foreach (var keyValuePair in __instance.m_locationInstances) {
          if (keyValuePair.Value.m_placed || !Operations.IncludedIds.Contains(keyValuePair.Value.m_location.m_prefabName.ToLower())) {
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
      return !Operations.PlaceToAlreadyGenerated || Operations.IncludedIds.Contains(location.m_prefabName.ToLower());
    }
  }
}