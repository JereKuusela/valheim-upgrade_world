using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Moves given locations to the positions of a freshly generated world.</summary>
public class RestoreLocations : ExecutedOperation
{
  readonly HashSet<string> Ids;
  readonly FiltererParameters Args;
  readonly bool Explored;
  int Added = 0;
  int Moved = 0;
  int Removed = 0;
  int Replaced = 0;
  int Skipped = 0;

  public RestoreLocations(Terminal context, HashSet<string> ids, FiltererParameters args, bool explored) : base(context, args.Pin)
  {
    Ids = ids;
    Explored = explored;
    Args = new(args)
    {
      TargetZones = TargetZones.All,
      Explored = explored ? null : false
    };
  }

  protected override void OnStart()
  {
    new PlayerBaseFilterer(Args.SafeZones).CalculateExcluded();
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    var zs = ZoneSystem.instance;
    var old = new Dictionary<Vector2s, ZoneSystem.LocationInstance>(zs.m_locationInstances);
    var done = false;
    try
    {
      Rebuild([]);
      DistributeLocations.AllowedZones = [.. Zones.GetAllZones()];
      // Same order and filtering as the vanilla generation.
      var ordered = zs.m_locations
        .Where(Helper.IsValid)
        .Where(loc => loc.m_enable && loc.m_quantity != 0)
        .OrderByDescending(loc => loc.m_prioritized)
        .ToList();
      Print("Generating a fresh location registry. This may take a while...");
      if (LPA.IsAvailable())
        yield return LPA.RunCustomPlacement(ordered.ToDictionary(loc => loc, loc => loc.m_quantity), DistributeLocations.AllowedZones);
      else
      {
        foreach (var location in ordered)
          yield return DistributeLocations.GenerateLocationsTimeSliced(this, location, sw);
      }
      if (Hud.instance)
        Hud.instance.m_loadingIndicator.SetShowProgress(false);
      var fresh = new Dictionary<Vector2s, ZoneSystem.LocationInstance>(zs.m_locationInstances);
      // Filters that depend on the registry must see the old one.
      zs.m_locationInstances = new(old);
      var merged = Merge(old, fresh);
      Rebuild(merged);
      done = true;
    }
    finally
    {
      if (!done) Rebuild(old);
    }
  }

  private Dictionary<Vector2s, ZoneSystem.LocationInstance> Merge(Dictionary<Vector2s, ZoneSystem.LocationInstance> old, Dictionary<Vector2s, ZoneSystem.LocationInstance> fresh)
  {
    var zs = ZoneSystem.instance;
    var merged = new Dictionary<Vector2s, ZoneSystem.LocationInstance>(old);
    var zones = old.Where(kvp => Ids.Contains(kvp.Value.m_location.m_prefab.Name)).Select(kvp => kvp.Key)
      .Concat(fresh.Where(kvp => Ids.Contains(kvp.Value.m_location.m_prefab.Name)).Select(kvp => kvp.Key))
      .Distinct().ToList();
    foreach (var zone in zones)
    {
      if (!Explored && zs.m_generatedZones.Contains(zone)) continue;
      var hasOld = old.TryGetValue(zone, out var oldInstance);
      var hasFresh = fresh.TryGetValue(zone, out var freshInstance);
      var oldTarget = hasOld && Ids.Contains(oldInstance.m_location.m_prefab.Name);
      var freshTarget = hasFresh && Ids.Contains(freshInstance.m_location.m_prefab.Name);
      if (oldTarget && !Args.FilterPosition(oldInstance.m_position, true)) continue;
      if (freshTarget && !Args.FilterPosition(freshInstance.m_position, true)) continue;
      if (oldTarget && freshTarget && oldInstance.m_location == freshInstance.m_location && Utils.DistanceXZ(oldInstance.m_position, freshInstance.m_position) < 1f) continue;
      if (hasOld && oldInstance.m_placed && !Explored)
      {
        if (freshTarget) Skipped++;
        continue;
      }
      if (!Args.Roll()) continue;
      if (hasOld && oldInstance.m_placed)
        RemoveObjects(zone, oldInstance);
      if (freshTarget)
      {
        freshInstance.m_placed = false;
        merged[zone] = freshInstance;
        AddPin(freshInstance.m_position);
        if (!hasOld) Added++;
        else if (oldTarget) Moved++;
        else Replaced++;
        if (Settings.Verbose)
          Print($"Location {freshInstance.m_location.m_prefab.Name} restored at {zone}.");
      }
      else
      {
        merged.Remove(zone);
        AddPin(oldInstance.m_position);
        Removed++;
        if (Settings.Verbose)
          Print($"Location {oldInstance.m_location.m_prefab.Name} removed at {zone}.");
      }
    }
    return merged;
  }

  private void RemoveObjects(Vector2s zone, ZoneSystem.LocationInstance instance)
  {
    var location = instance.m_location;
    var radius = Args.ObjectReset ?? location.m_exteriorRadius;
    var proxy = Helper.GetZDOs(zone)?.FirstOrDefault(zdo => zdo.m_prefab == LocationProxyHash && zdo.GetInt(LocationHash) == location.Hash);
    Helper.ClearZDOsWithinDistance(zone, proxy?.GetPosition() ?? instance.m_position, radius);
    if (proxy != null) Helper.RemoveZDO(proxy);
    ResetTerrain.Execute(instance.m_position, Args.TerrainReset == 0f ? location.m_exteriorRadius : Args.TerrainReset);
  }

  // Also resets the vanilla lookup caches, which are used by the generation.
  private static void Rebuild(Dictionary<Vector2s, ZoneSystem.LocationInstance> instances)
  {
    var zs = ZoneSystem.instance;
    zs.m_locationInstances = [];
    zs.m_locationIDCache.Clear();
    zs.m_locationGroupCache.Clear();
    zs.m_locationMaxGroupCache.Clear();
    foreach (var instance in instances.Values)
      zs.RegisterLocation(instance.m_location, instance.m_position, instance.m_placed);
  }

  protected override void OnEnd()
  {
    Print($"Restored locations{LocationOperation.IdString(Ids)}: {Added} added, {Moved} moved, {Replaced} replaced other locations, {Removed} removed.");
    if (Skipped > 0)
      Print($"{Skipped} locations were skipped because the zone has an existing location. Use explored=true to replace them.");
  }

  protected override string OnInit()
  {
    return Args.Print($"Restore locations{LocationOperation.IdString(Ids)} to their original places in");
  }
}
