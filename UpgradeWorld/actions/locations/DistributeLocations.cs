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
      var zs = ZoneSystem.instance;
      var locations = zs.m_locations.Where(location => Helper.IsValid(location) && location.m_prefab.Name == id).ToArray();
      if (locations.Length == 0)
        continue;
      Print($"Generating locations {id}. This may take a while...");
      var before = Counts.TryGetValue(id, out var count) ? count : 0;
      foreach (var location in locations)
        yield return GenerateLocationsTimeSliced(location, sw);
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
    return zs.m_locationInstances.Count(kvp => kvp.Value.m_location.m_prefab.Name == id);
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

  // Copy paste from Valheim to allow overriding IsZoneGenerated check.
  private IEnumerator GenerateLocationsTimeSliced(ZoneSystem.ZoneLocation location, Stopwatch timeSliceStopwatch)
  {
    var zs = ZoneSystem.instance;
    int seed = WorldGenerator.instance.GetSeed() + location.m_prefab.Name.GetStableHashCode();
    UnityEngine.Random.State state = UnityEngine.Random.state;
    UnityEngine.Random.InitState(seed);
    int errorLocationInZone = 0;
    int errorCenterDistance = 0;
    int errorBiome = 0;
    int errorBiomeArea = 0;
    int errorAlt = 0;
    int errorForest = 0;
    int errorSimilar = 0;
    int errorNotSimilar = 0;
    int errorTerrainDelta = 0;
    int errorVegetation = 0;
    float maxRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
    int attempts = location.m_prioritized ? 200000 : 100000;
    int iterations = 0;
    int placed = zs.CountNrOfLocation(location);
    float maxRange = 10000f;
    if (location.m_centerFirst)
    {
      maxRange = location.m_minDistance;
    }
    if (!location.m_unique || placed <= 0)
    {
      zs.s_tempVeg.Clear();
      int i = 0;
      while (i < attempts && placed < location.m_quantity)
      {
        if (timeSliceStopwatch.Elapsed.TotalSeconds >= zs.m_timeSlicedGenerationTimeBudget)
        {
          UnityEngine.Random.State insideState = UnityEngine.Random.state;
          UnityEngine.Random.state = state;
          yield return null;
          timeSliceStopwatch.Restart();
          state = UnityEngine.Random.state;
          UnityEngine.Random.state = insideState;
        }
        Vector2i zoneID = ZoneSystem.GetRandomZone(maxRange);
        if (location.m_centerFirst)
        {
          maxRange += 1f;
        }
        int num;
        if (zs.m_locationInstances.ContainsKey(zoneID))
        {
          num = errorLocationInZone + 1;
          errorLocationInZone = num;
        }
        else if (AllowedZones.Contains(zoneID))
        {
          Vector3 zonePos = ZoneSystem.GetZonePos(zoneID);
          Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zonePos);
          if ((location.m_biomeArea & biomeArea) == 0)
          {
            num = errorBiomeArea + 1;
            errorBiomeArea = num;
          }
          else
          {
            for (int j = 0; j < 20; j = num)
            {
              if (timeSliceStopwatch.Elapsed.TotalSeconds >= zs.m_timeSlicedGenerationTimeBudget)
              {
                UnityEngine.Random.State insideState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
                yield return null;
                timeSliceStopwatch.Restart();
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = insideState;
              }
              num = iterations + 1;
              iterations = num;
              Vector3 randomPointInZone = ZoneSystem.GetRandomPointInZone(zoneID, maxRadius);
              float magnitude = randomPointInZone.magnitude;
              if (location.m_minDistance != 0f && magnitude < location.m_minDistance)
              {
                num = errorCenterDistance + 1;
                errorCenterDistance = num;
              }
              else if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance)
              {
                num = errorCenterDistance + 1;
                errorCenterDistance = num;
              }
              else
              {
                Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
                if ((location.m_biome & biome) == Heightmap.Biome.None)
                {
                  num = errorBiome + 1;
                  errorBiome = num;
                }
                else
                {
                  Color color;
                  randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z, out color);
                  float num2 = (float)(randomPointInZone.y - 30.0);
                  if (num2 < location.m_minAltitude || num2 > location.m_maxAltitude)
                  {
                    num = errorAlt + 1;
                    errorAlt = num;
                  }
                  else
                  {
                    if (location.m_inForest)
                    {
                      float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
                      if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax)
                      {
                        num = errorForest + 1;
                        errorForest = num;
                        goto IL_7F2;
                      }
                    }
                    if (location.m_minDistanceFromCenter > 0f || location.m_maxDistanceFromCenter > 0f)
                    {
                      float num3 = Utils.LengthXZ(randomPointInZone);
                      if ((location.m_minDistanceFromCenter > 0f && num3 < location.m_minDistanceFromCenter) || (location.m_maxDistanceFromCenter > 0f && num3 > location.m_maxDistanceFromCenter))
                      {
                        goto IL_7F2;
                      }
                    }
                    WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out var num4, out _);
                    if (num4 > location.m_maxTerrainDelta || num4 < location.m_minTerrainDelta)
                    {
                      num = errorTerrainDelta + 1;
                      errorTerrainDelta = num;
                    }
                    else if (location.m_minDistanceFromSimilar > 0f && zs.HaveLocationInRange(location.m_prefab.Name, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar, false))
                    {
                      num = errorSimilar + 1;
                      errorSimilar = num;
                    }
                    else if (location.m_maxDistanceFromSimilar > 0f && !zs.HaveLocationInRange(location.m_prefab.Name, location.m_groupMax, randomPointInZone, location.m_maxDistanceFromSimilar, true))
                    {
                      num = errorNotSimilar + 1;
                      errorNotSimilar = num;
                    }
                    else
                    {
                      float a = color.a;
                      if (location.m_minimumVegetation > 0f && a <= location.m_minimumVegetation)
                      {
                        num = errorVegetation + 1;
                        errorVegetation = num;
                      }
                      else
                      {
                        if (location.m_maximumVegetation >= 1f || a < location.m_maximumVegetation)
                        {
                          if (location.m_surroundCheckVegetation)
                          {
                            float num5 = 0f;
                            for (int k = 0; k < location.m_surroundCheckLayers; k++)
                            {
                              float num6 = (k + 1) / (float)location.m_surroundCheckLayers * location.m_surroundCheckDistance;
                              for (int l = 0; l < 6; l++)
                              {
                                float f = l / 6f * 3.1415927f * 2f;
                                Vector3 vector2 = randomPointInZone + new Vector3(Mathf.Sin(f) * num6, 0f, Mathf.Cos(f) * num6);
                                Color color2;
                                WorldGenerator.instance.GetHeight(vector2.x, vector2.z, out color2);
                                float num7 = (location.m_surroundCheckDistance - num6) / (location.m_surroundCheckDistance * 2f);
                                num5 += color2.a * num7;
                              }
                            }
                            zs.s_tempVeg.Add(num5);
                            if (zs.s_tempVeg.Count < 10)
                            {
                              goto IL_7F2;
                            }
                            float num8 = zs.s_tempVeg.Max();
                            float num9 = zs.s_tempVeg.Average();
                            float num10 = num9 + (num8 - num9) * location.m_surroundBetterThanAverage;
                            if (num5 < num10)
                            {
                              goto IL_7F2;
                            }
                          }
                          zs.RegisterLocation(location, randomPointInZone, false);
                          num = placed + 1;
                          placed = num;
                          break;
                        }
                        num = errorVegetation + 1;
                        errorVegetation = num;
                      }
                    }
                  }
                }
              }
            IL_7F2:
              num = j + 1;
            }
          }
        }
        num = i + 1;
        i = num;
      }
      if (placed < location.m_quantity)
      {
        Print($"Failed to place all {location.m_prefab.Name}, placed {placed} out of {location.m_quantity}.");
        if (Settings.VerboseLocations)
        {
          if (errorLocationInZone > 0)
            Print("Failed " + errorLocationInZone + " times because the zone already had a location.");
          if (errorCenterDistance > 0)
            Print("Failed " + errorCenterDistance + " times because the location had wrong distance from the world center.");
          if (errorBiome > 0)
            Print("Failed " + errorBiome + " times because the biome was wrong.");
          if (errorBiomeArea > 0)
            Print("Failed " + errorBiomeArea + " times because the biome area was wrong.");
          if (errorAlt > 0)
            Print("Failed " + errorAlt + " times because the altitude was wrong.");
          if (errorForest > 0)
            Print("Failed " + errorForest + " times because the forest factor was wrong.");
          if (errorSimilar > 0)
            Print("Failed " + errorSimilar + " times because there was a similar location too close.");
          if (errorNotSimilar > 0)
            Print("Failed " + errorNotSimilar + " times because there was no similar location close enough.");
          if (errorTerrainDelta > 0)
            Print("Failed " + errorTerrainDelta + " times because the terrain delta was too high.");
          if (errorVegetation > 0)
            Print("Failed " + errorVegetation + " times because the vegetation factor was wrong.");
        }
      }
    }
    UnityEngine.Random.state = state;
    yield break;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ClearNonPlacedLocations))]
public class DummyClearNonPlacedLocations
{
  public static bool Skip = false;
  static bool Prefix() => !Skip;
}
