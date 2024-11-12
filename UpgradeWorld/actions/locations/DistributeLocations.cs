using System;
using System.Collections.Generic;
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
    Ids = [.. ids];
    if (Ids.Length == 0)
      Ids = ZoneSystem.instance.m_locations.Where(loc => Helper.IsValid(loc) && loc.m_enable && loc.m_quantity != 0).OrderByDescending(loc => loc.m_prioritized).Select(loc => loc.m_prefab.Name).ToArray();
    Chance = args.Chance;
    args = new(args)
    {
      TargetZones = TargetZones.All
    };
    var filterers = FiltererFactory.Create(args);
    List<string> messages = [];
    AllowedZones = [.. filterers.Aggregate(Zones.GetZones(args), (zones, filterer) => filterer.FilterZones(zones, ref messages)).Distinct()];
  }
  protected override bool OnExecute()
  {
    // Note: Printing is done one step before the execution, otherwise it would get printed afterwards.
    if (Attempts == 0)
    {
      LoadingIndicator.SetProgressVisibility(true);
      Print($"Generating locations {Ids[Attempts]}. This may take a while...");
      ClearNotSpawned(Ids);
      return false;
    }
    if (Attempts <= Ids.Length)
    {
      SpawnToAlreadyGenerated = true;
      var zs = ZoneSystem.instance;
      var id = Ids[Attempts - 1];
      var locations = zs.m_locations.Where(location => Helper.IsValid(location) && location.m_prefab.Name == id).ToArray();
      if (locations.Length == 0) return false;
      // Note: Printing is done one step before the execution, otherwise it would get printed afterwards.
      if (Attempts < Ids.Length)
        Print($"Generating locations {Ids[Attempts]}. This may take a while...");
      var before = Counts.TryGetValue(id, out var count) ? count : 0;
      foreach (var location in locations)
        GenerateLocations(location);
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
      LoadingIndicator.SetProgress(Attempts / (float)Ids.Length);
      return false;
    }
    LoadingIndicator.SetProgressVisibility(false);
    return true;
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
      Print($"{Added} locations{Helper.LocationIdString(Ids)} added to the world (total amount in the world: {Total}).");
    else
      Print($"{Math.Abs(Added)} locations{Helper.LocationIdString(Ids)} removed from the world (total amount in the world: {Total}).");
  }


  protected override string OnInit()
  {
    return $"Generate locations{Helper.LocationIdString(Ids)}.";
  }

  // Copy paste for now.
  private void GenerateLocations(ZoneSystem.ZoneLocation location)
  {
    ZoneSystem zs = ZoneSystem.instance;
    int seed = WorldGenerator.instance.GetSeed() + StringExtensionMethods.GetStableHashCode(location.m_prefab.Name);
    UnityEngine.Random.State state = UnityEngine.Random.state;
    UnityEngine.Random.InitState(seed);
    float maxRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
    int attempts = location.m_prioritized ? 200000 : 100000;
    int placed = zs.CountNrOfLocation(location);
    if (location.m_unique && placed > 0) return;
    float maxRange = Settings.WorldRadius - Settings.WorldEdge;
    if (location.m_centerFirst)
      maxRange = location.m_minDistance;

    zs.s_tempVeg.Clear();
    int i = 0;
    while (i < attempts && placed < location.m_quantity)
    {
      i += 1;
      Vector2i zoneID = ZoneSystem.GetRandomZone(maxRange);
      if (location.m_centerFirst)
        maxRange += 1f;
      if (zs.m_locationInstances.ContainsKey(zoneID)) continue;
      if (zs.IsZoneGenerated(zoneID)) continue;
      Vector3 zonePos = ZoneSystem.GetZonePos(zoneID);
      Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zonePos);
      if ((location.m_biomeArea & biomeArea) == 0) continue;

      for (int j = 0; j < 20; j += 1)
      {
        Vector3 randomPointInZone = ZoneSystem.GetRandomPointInZone(zoneID, maxRadius);
        float magnitude = randomPointInZone.magnitude;
        if (location.m_minDistance != 0f && magnitude < location.m_minDistance) continue;
        if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance) continue;
        Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
        if ((location.m_biome & biome) == Heightmap.Biome.None) continue;

        randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z, out var color);
        float num2 = (float)(randomPointInZone.y - 30.0);
        if (num2 < location.m_minAltitude || num2 > location.m_maxAltitude) continue;

        if (location.m_inForest)
        {
          float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
          if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax) continue;
        }
        WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out var num3, out var vector);
        if (num3 > location.m_maxTerrainDelta || num3 < location.m_minTerrainDelta) continue;
        if (location.m_minDistanceFromSimilar > 0f && zs.HaveLocationInRange(location.m_prefab.Name, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar, false)) continue;
        if (location.m_maxDistanceFromSimilar > 0f && !zs.HaveLocationInRange(location.m_prefabName, location.m_groupMax, randomPointInZone, location.m_maxDistanceFromSimilar, true)) continue;

        float a = color.a;
        if (location.m_minimumVegetation > 0f && a <= location.m_minimumVegetation) continue;
        if (location.m_maximumVegetation < 1f && a >= location.m_maximumVegetation) continue;

        if (location.m_surroundCheckVegetation)
        {
          float num4 = 0f;
          for (int k = 0; k < location.m_surroundCheckLayers; k++)
          {
            float num5 = (k + 1) / (float)location.m_surroundCheckLayers * location.m_surroundCheckDistance;
            for (int l = 0; l < 6; l++)
            {
              float f = l / 6f * 3.1415927f * 2f;
              Vector3 vector2 = randomPointInZone + new Vector3(Mathf.Sin(f) * num5, 0f, Mathf.Cos(f) * num5);
              WorldGenerator.instance.GetHeight(vector2.x, vector2.z, out var color2);
              float num6 = (location.m_surroundCheckDistance - num5) / (location.m_surroundCheckDistance * 2f);
              num4 += color2.a * num6;
            }
          }
          zs.s_tempVeg.Add(num4);
          if (zs.s_tempVeg.Count < 10) continue;
          float num7 = zs.s_tempVeg.Max();
          float num8 = zs.s_tempVeg.Average();
          float num9 = num8 + (num7 - num8) * location.m_surroundBetterThanAverage;
          if (num4 < num9) continue;
        }
        zs.RegisterLocation(location, randomPointInZone, false);
        placed += 1;
        break;
      }
    }
    UnityEngine.Random.state = state;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ClearNonPlacedLocations))]
public class DummyClearNonPlacedLocations
{
  public static bool Skip = false;
  static bool Prefix() => !Skip;
}
