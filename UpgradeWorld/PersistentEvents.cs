using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;

/// <summary>Persistent events are stored outside ZDOs, and their objects have no marker so they are matched by prefab and distance.</summary>
public static class PersistentEvents
{
  public static List<string> Names()
  {
    var system = PersistentEventSystem.instance;
    if (system == null) return [];
    return [.. system.m_possibleEvents.Select(e => e.internalName).Distinct()];
  }

  public static List<PersistentEventSystem.ActivePersistentEvent> Active()
  {
    var system = PersistentEventSystem.instance;
    if (system == null || system.m_activePersistentEvents?.list == null) return [];
    return [.. system.m_activePersistentEvents.list.Where(e => e.sourceEventId >= 0 && e.sourceEventId < system.m_possibleEvents.Count)];
  }

  /// <summary>Selects events by name and by the filters applied to the event center.</summary>
  public static List<PersistentEventSystem.ActivePersistentEvent> Select(IdParameters args)
  {
    PlayerBaseFilterer.Update(args.SafeZones);
    var ids = args.Ids();
    return [.. Active().Where(e =>
      (ids.Count == 0 || ids.Any(id => Helper.IsIncluded(id, e.internalName))) &&
      !args.Ignore.Any(id => Helper.IsIncluded(id, e.internalName)) &&
      args.FilterPosition(e.position, true))];
  }

  private static float Spread(PersistentEventSystem.ObjectSpawnSettings spawn, PersistentEventSystem.ActivePersistentEvent e)
  {
    // Same rule as ObjectSpawnSettings.Create.
    return spawn.maxDistanceFromEventCenter <= 0.01f ? e.radius : spawn.maxDistanceFromEventCenter;
  }

  /// <summary>Distance from the event center which can contain its objects.</summary>
  public static float Reach(PersistentEventSystem.ActivePersistentEvent e)
  {
    var reach = e.radius;
    foreach (var spawn in e.Source.objectsToSpawn)
      reach = Mathf.Max(reach, Spread(spawn, e));
    return reach;
  }

  public static bool Touches(PersistentEventSystem.ActivePersistentEvent e, HashSet<Vector2s> zones)
  {
    var reach = Reach(e);
    var half = ZoneSystem.instance.m_zoneSize / 2f;
    var min = ZoneSystem.GetZone(new Vector3(e.position.x - reach, 0, e.position.z - reach));
    var max = ZoneSystem.GetZone(new Vector3(e.position.x + reach, 0, e.position.z + reach));
    for (int x = min.x; x <= max.x; x++)
    {
      for (int y = min.y; y <= max.y; y++)
      {
        var zone = new Vector2s(x, y);
        if (!zones.Contains(zone)) continue;
        var center = ZoneSystem.GetZonePos(zone);
        var dx = Mathf.Max(Mathf.Abs(e.position.x - center.x) - half, 0f);
        var dz = Mathf.Max(Mathf.Abs(e.position.z - center.z) - half, 0f);
        if (dx * dx + dz * dz <= reach * reach) return true;
      }
    }
    return false;
  }

  private static List<ZDO> FindObjects(PersistentEventSystem.ActivePersistentEvent e)
  {
    var found = new HashSet<ZDO>();
    foreach (var spawn in e.Source.objectsToSpawn)
    {
      if (spawn.prefabs == null) continue;
      HashSet<int> hashes = [.. spawn.prefabs.Where(prefab => prefab).Select(prefab => prefab.name.GetStableHashCode())];
      if (hashes.Count == 0) continue;
      var reach = Spread(spawn, e) + 1f;
      foreach (var zdo in ZDOMan.instance.m_objectsByID.Values)
      {
        if (!hashes.Contains(zdo.m_prefab)) continue;
        if (Utils.DistanceXZ(zdo.GetPosition(), e.position) <= reach) found.Add(zdo);
      }
    }
    return [.. found];
  }

  /// <summary>Removes events with their objects and their not yet placed location. Returns the amount of removed objects.</summary>
  public static int Remove(IEnumerable<PersistentEventSystem.ActivePersistentEvent> events)
  {
    var system = PersistentEventSystem.instance;
    var zs = ZoneSystem.instance;
    var objects = 0;
    var changed = false;
    foreach (var e in events.ToList())
    {
      foreach (var zdo in FindObjects(e))
      {
        Helper.RemoveZDO(zdo);
        objects++;
      }
      var name = e.Source.locationToSpawn;
      var zone = ZoneSystem.GetZone(e.position);
      // Placed locations are left for locations_remove.
      // Note: Vanilla Jotunn invasion doesn't add location so this isn't super important.
      if (!string.IsNullOrWhiteSpace(name) && zs.m_locationInstances.TryGetValue(zone, out var location) && !location.m_placed && location.m_location.m_prefab.Name == name)
        zs.m_locationInstances.Remove(zone);
      changed |= system.m_activePersistentEvents.list.Remove(e);
    }
    if (changed) system.UpdateClientEventsList();
    return objects;
  }
}
