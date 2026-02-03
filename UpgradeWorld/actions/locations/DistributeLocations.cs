using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;
/// <summary>Distributes given location ids to already generated zones.</summary>
public class DistributeLocations : ExecutedOperation
{
  private readonly string[] Ids = [];
  public float Chance = 1f;
  public int Added = 0;
  private int Total = 0;
  public static bool SpawnToAlreadyGenerated = false;
  public static HashSet<Vector2i> AllowedZones = [];
  private readonly Dictionary<string, int> Counts = [];
  public DistributeLocations(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args.Start)
  {
    Ids = [.. ZoneSystem.instance.m_locations
        .Where(Helper.IsValid)
        .Where(loc => ids.Contains(loc.m_prefab.Name))
        .Where(loc => loc.m_enable && loc.m_quantity != 0)
        .Where(loc => (loc.m_biome & args.BiomeMask) != Heightmap.Biome.None)
        .OrderByDescending(loc => loc.m_prioritized)
        .Select(loc => loc.m_prefab.Name)];
    if (Ids.Length == 0)
    {
      Print("Error: No valid location ids.");
      return;
    }
    Chance = args.Chance;
    args = new(args)
    {
      TargetZones = TargetZones.All
    };
    var filterers = FiltererFactory.Create(args);
    List<string> messages = [];
    AllowedZones = [.. filterers.Aggregate(Zones.GetZones(args), (zones, filterer) => filterer.FilterZones(zones, ref messages)).Distinct()];
  }
  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    if (Ids.Length == 0)
    {
      Print("No locations to generate.");
      yield break;
    }
    var counter = 0;
    foreach (var id in Ids)
    {
      counter += 1;
      ClearNotSpawned(Ids);
      SpawnToAlreadyGenerated = true;
      var zs = ZoneSystem.instance;
      var locations = zs.m_locations.Where(location => Helper.IsValid(location) && location.m_prefab.Name == id).ToArray();
      if (locations.Length == 0)
        continue;
      Print($"Generating locations {id}. This may take a while...");
      var before = Counts.TryGetValue(id, out var count) ? count : 0;
      ZPackage pkg = new();
      foreach (var location in locations)
        yield return zs.GenerateLocationsTimeSliced(location, sw, pkg);
      if (Chance < 1f)
      {
        zs.m_locationInstances = zs.m_locationInstances
          .Where(kvp => kvp.Value.m_placed || kvp.Value.m_location.m_prefab.Name != id || FiltererParameters.random.NextDouble() < Chance)
          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      }
      var unplaced = zs.m_locationInstances.Where(kvp => kvp.Value.m_location.m_prefab.Name == id && !kvp.Value.m_placed).ToList();
      foreach (var kvp in unplaced) AddPin(kvp.Value.m_position);
      Total += Count(id);
      Added += Total - before;
      SpawnToAlreadyGenerated = false;
    }
    // Needed to indicate end of generation for some mods.
    if (Hud.instance)
      Hud.instance.m_loadingIndicator.SetShowProgress(false);
  }
  private void ClearNotSpawned(string[] ids)
  {
    Counts.Clear();
    foreach (var id in ids)
      Counts[id] = Count(id);
    var zs = ZoneSystem.instance;
    zs.m_locationInstances = zs.m_locationInstances.Where(kvp => kvp.Value.m_placed || !Counts.ContainsKey(kvp.Value.m_location.m_prefab.Name)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    // Better Continents adds its own locations after ClearNonPlacedLocations is called.
    // So a workaround is to make a dummy call.
    DummyClearNonPlacedLocations.Skip = true;
    zs.ClearNonPlacedLocations();
    DummyClearNonPlacedLocations.Skip = false;
  }
  private int Count(string id)
  {
    var zs = ZoneSystem.instance;
    return zs.m_locationInstances.Where(kvp => kvp.Value.m_location.m_prefab.Name == id).Count();
  }
  protected override void OnEnd()
  {
    if (Added >= 0)
      Print($"{Added} locations{LocationOperation.IdString(Ids)} added to the world (total amount in the world: {Total}).");
    else
      Print($"{Math.Abs(Added)} locations{LocationOperation.IdString(Ids)} removed from the world (total amount in the world: {Total}).");
  }


  protected override string OnInit()
  {
    return $"Generate locations{LocationOperation.IdString(Ids)}.";
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ClearNonPlacedLocations))]
public class DummyClearNonPlacedLocations
{
  public static bool Skip = false;
  static bool Prefix() => !Skip;
}
