using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {

  public partial class Operation {
    /// <summary>Destroys everything in zone if it's not loaded.</summary>
    public static bool Upgrade(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      if (zoneSystem.IsZoneLoaded(zone)) {
        return true;
      }
      Patch.ZoneSystem_PokeLocalZone(zoneSystem, zone);
      if (!zoneSystem.IsZoneLoaded(zone)) {
        return false;
      }
      var mZones = Patch.GetZones(zoneSystem);
      var root = Zones.GetRoot(mZones[zone]);
      spawnedObjects.Clear();
      Object.Destroy(root);
      mZones.Remove(zone);
      return true;
    }
    public static void DoIt(Vector2i[] zones, int version) {
      var zoneSystem = ZoneSystem.instance;
      var mZones = Patch.GetZones(zoneSystem);
      foreach (var zone in zones) {
        var shouldBeUnloaded = Patch.ZoneSystem_PokeLocalZone(zoneSystem, zone);
        var mode = shouldBeUnloaded ? ZoneSystem.SpawnMode.Ghost : ZoneSystem.SpawnMode.Full;
        var obj = mZones[zone];
        var root = Patch.Get<GameObject>(obj, "m_root");
        UpgradeVeg(zone, version);
        PlaceLocation(zone, root, mode);
        if (shouldBeUnloaded) {
          foreach (GameObject spawned in spawnedObjects) {
            Object.Destroy(spawned);
          }
          spawnedObjects.Clear();
          Object.Destroy(root);
          mZones.Remove(zone);
        }
      }
    }
    // Returns a array of zones from the center zone towards edge zones within a given adjacency.
    public static void UpgradeVeg(Vector2i zoneID, int version) {
      Vector3 zonePos = ZoneSystem.instance.GetZonePos(zoneID);
      string item = "StatueCorgi";
      int range = 20;
      UnityEngine.Random.State state = UnityEngine.Random.state;
      int seed = WorldGenerator.instance.GetSeed();
      UnityEngine.Random.InitState(seed + (zoneID.x * 4271) + (zoneID.y * 9187) + item.GetStableHashCode());
      GameObject prefab = ZNetScene.instance.GetPrefab(item);
      Vector3 position = new Vector3(UnityEngine.Random.Range(zonePos.x - range, zonePos.x + range), 0f, UnityEngine.Random.Range(zonePos.z - range, zonePos.z + range));
      ZoneSystem.instance.GetGroundData(ref position, out Vector3 normal, out _, out _, out _);
      float angle = UnityEngine.Random.Range(0, 360);
      Quaternion rotation = Quaternion.AngleAxis(angle, normal);

      GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
      ZNetView component = gameObject.GetComponent<ZNetView>();
      component.GetZDO().SetPGWVersion(version);
      UnityEngine.Random.state = state;
    }
    private static readonly List<object> clearAreas = new List<object>();
    private static readonly List<GameObject> spawnedObjects = new List<GameObject>();
    // Base game code only places non-placed locations so this can be ran safely.
    public static void PlaceLocation(Vector2i zoneID, GameObject root, ZoneSystem.SpawnMode mode) {
      var zoneSystem = ZoneSystem.instance;
      Vector3 zonePos = ZoneSystem.instance.GetZonePos(zoneID);
      Heightmap heightmap = root.GetComponentInChildren<Heightmap>();
      clearAreas.Clear();
      spawnedObjects.Clear();
      Patch.ZoneSystem_PlaceLocations(zoneSystem, zoneID, zonePos, root.transform, heightmap, clearAreas, mode, spawnedObjects);
    }
  }
}