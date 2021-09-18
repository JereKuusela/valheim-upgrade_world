using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace UpgradeWorld {
  [HarmonyPatch]
  public class Patch {
    public static T Get<T>(object obj, string field) {
      return Traverse.Create(obj).Field<T>(field).Value;
    }

    public static HashSet<Vector2i> GetGeneratedZones(ZoneSystem obj) {
      return Get<HashSet<Vector2i>>(obj, "m_generatedZones");
    }
    public static IDictionary GetZones(ZoneSystem obj) {
      return Get<IDictionary>(obj, "m_zones");
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ZoneSystem), "IsZoneGenerated")]
    public static bool ZoneSystem_IsZoneGenerated(ZoneSystem instance, Vector2i zoneId) {
      throw new NotImplementedException("Dummy");
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ZoneSystem), "PokeLocalZone")]
    public static bool ZoneSystem_PokeLocalZone(ZoneSystem instance, Vector2i zoneId) {
      throw new NotImplementedException("Dummy");
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ZoneSystem), "PlaceLocations")]
    public static void ZoneSystem_PlaceLocations(ZoneSystem instance, Vector2i zoneId, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, IList<object> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects) {
      throw new NotImplementedException("Dummy");
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ZDOMan), "FindObjects")]
    public static void ZDOMan_FindObjects(ZDOMan instance, Vector2i sector, List<ZDO> objects) {
      throw new NotImplementedException("Dummy");
    }
  }
}