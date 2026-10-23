using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld;
// Distributes given location ids to already generated zones
public class DistributeLocations : ExecutedOperation
{
  private readonly string[] Ids = [];
  public float Chance = 1f;
  public int Added = 0;
  private int Total = 0;
  public static HashSet<Vector2s> AllowedZones = [];
  private readonly Dictionary<string, int> Counts = [];
  private readonly bool? Explored;
  public DistributeLocations(Terminal context, HashSet<string> ids, FiltererParameters args, bool? explored = null) : base(context, args.Pin)
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
    Explored = explored;
    args = new(args)
    {
      TargetZones = explored switch
      {
        true => TargetZones.Generated,
        false => TargetZones.Ungenerated,
        _ => TargetZones.All
      }
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
    ClearNotSpawned(Ids);

    var zs = ZoneSystem.instance;


    // If LPA is present, hand spatial allocation to it otherwise run UW's own generation loop. 
    if (LPA.IsAvailable())
    {
      Print("LPA detected. Handing off spatial allocation.");
      var lpaRequests = new Dictionary<ZoneSystem.ZoneLocation, int>();
      foreach (var id in Ids)
      {
        foreach (var location in zs.m_locations.Where(loc => Helper.IsValid(loc) && loc.m_prefab.Name == id))
        {
          lpaRequests[location] = location.m_quantity;
        }
      }
      if (lpaRequests.Count > 0)
      {
        yield return LPA.RunCustomPlacement(lpaRequests, AllowedZones);
      }
    }
    else
    {
      var counter = 0;
      foreach (var id in Ids)
      {
        counter += 1;
        var locations = zs.m_locations.Where(location => Helper.IsValid(location) && location.m_prefab.Name == id).ToArray();
        if (locations.Length == 0)
          continue;
        Print($"Generating locations {id}. This may take a while...");
        foreach (var location in locations)
          yield return GenerateLocationsTimeSliced(this, location, sw);
      }
    }

    // Post-processing should be identical for both paths.
    foreach (var id in Ids)
    {
      var before = Counts.TryGetValue(id, out var count) ? count : 0;
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
    zs.m_locationInstances = zs.m_locationInstances.Where(kvp => kvp.Value.m_placed || !Counts.ContainsKey(kvp.Value.m_location.m_prefab.Name) || (Explored.HasValue && !AllowedZones.Contains(kvp.Key))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
    return $"Generate locations{LocationOperation.IdString(Ids)}{(Explored.HasValue ? " in " + FiltererParameters.ZonesText(Explored) : "")}.";
  }

  // Copy of vanilla (keep in sync) using AllowedZones instead of IsZoneGenerated and different logging.
  internal static IEnumerator GenerateLocationsTimeSliced(BaseOperation op, ZoneSystem.ZoneLocation location, Stopwatch timeSliceStopwatch)
  {
    void Print(string value) => op.Print(value);
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
    int errorAltBiomeMissing = 0;
    int errorAltBiomeBlock = 0;
    float maxRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
    int attempts = location.m_prioritized ? 60000 : 12000;
    int iterations = 0;
    int placed = zs.CountNrOfLocation(location);
    float maxRange = 10000f;
    zs.s_tempVeg.Clear();
    if (!location.m_unique || placed <= 0)
    {
      if (location.m_centerFirst)
      {
        maxRange = location.m_minDistance;
      }
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
        Vector2s zoneID = location.m_centerFirst
          ? ZoneSystem.GetRandomZone(maxRange)
          : ZoneSystem.GetZone(AltBiomeWorldData.MapSpaceToWorldSpace(location.m_minAltitude < 0f
            ? ZNet.World.m_biomeData.GetRandomPointByBiomes(location.m_biome)
            : ZNet.World.m_biomeData.GetRandomPointByBiomesAboveSeaLevel(location.m_biome)));
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
          var zoneCenter = ZoneSystem.GetZoneCenter(zoneID);
          Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zoneCenter);
          if ((location.m_biomeArea & biomeArea) == 0)
          {
            num = errorBiomeArea + 1;
            errorBiomeArea = num;
          }
          else
          {
            for (int j = 0; j < 6; j++)
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
                errorCenterDistance++;
                continue;
              }
              if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance)
              {
                errorCenterDistance++;
                continue;
              }
              Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
              if ((location.m_biome & biome) == Heightmap.Biome.None)
              {
                errorBiome++;
                continue;
              }
              randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z, out var mask);
              float altitude = (float)(randomPointInZone.y - 30.0);
              if (altitude < location.m_minAltitude || altitude > location.m_maxAltitude)
              {
                errorAlt++;
                continue;
              }
              if (location.m_inForest)
              {
                float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
                if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax)
                {
                  errorForest++;
                  continue;
                }
              }
              if (location.m_minDistanceFromCenter > 0f || location.m_maxDistanceFromCenter > 0f)
              {
                float fromCenter = Utils.LengthXZ(randomPointInZone);
                if ((location.m_minDistanceFromCenter > 0f && fromCenter < location.m_minDistanceFromCenter) || (location.m_maxDistanceFromCenter > 0f && fromCenter > location.m_maxDistanceFromCenter))
                  continue;
              }
              WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out var delta, out _);
              if (delta > location.m_maxTerrainDelta || delta < location.m_minTerrainDelta)
              {
                errorTerrainDelta++;
                continue;
              }
              if (location.m_minDistanceFromSimilar > 0f && zs.HaveLocationInRange(location.m_prefab.m_assetID, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar))
              {
                errorSimilar++;
                continue;
              }
              if (location.m_maxDistanceFromSimilar > 0f && !zs.HaveLocationInRange(location.m_prefab.m_assetID, location.m_groupMax, randomPointInZone, location.m_maxDistanceFromSimilar, maxGroup: true))
              {
                errorNotSimilar++;
                continue;
              }
              float vegetation = mask.a;
              if (location.m_minimumVegetation > 0f && vegetation <= location.m_minimumVegetation)
              {
                errorVegetation++;
                continue;
              }
              if (location.m_maximumVegetation < 1f && vegetation >= location.m_maximumVegetation)
              {
                errorVegetation++;
                continue;
              }
              var biomeSector = WorldGenerator.instance.GetBiomeSector(randomPointInZone);
              if (location.AltBiomeParent != null && !biomeSector.AltBiomes.Any(x => x.m_name == location.AltBiomeParent))
              {
                errorAltBiomeMissing++;
                continue;
              }
              if (biomeSector.AltBiomes.Any(x => x.m_blockLocationNames.Contains(location.m_name)))
              {
                errorAltBiomeBlock++;
                continue;
              }
              if (location.m_surroundCheckVegetation)
              {
                float surround = 0f;
                for (int k = 0; k < location.m_surroundCheckLayers; k++)
                {
                  float distance = (k + 1) / (float)location.m_surroundCheckLayers * location.m_surroundCheckDistance;
                  for (int l = 0; l < 6; l++)
                  {
                    float f = (float)((double)l / 6.0 * (double)Mathf.PI * 2.0);
                    Vector3 point = randomPointInZone + new Vector3(Mathf.Sin(f) * distance, 0f, Mathf.Cos(f) * distance);
                    WorldGenerator.instance.GetHeight(point.x, point.z, out var mask2);
                    float weight = (float)(((double)location.m_surroundCheckDistance - (double)distance) / ((double)location.m_surroundCheckDistance * 2.0));
                    surround += mask2.a * weight;
                  }
                }
                zs.s_tempVeg.Add(surround);
                if (zs.s_tempVeg.Count < 10)
                  continue;
                float max = zs.s_tempVeg.Max();
                float average = zs.s_tempVeg.Average();
                float cutoff = average + (max - average) * location.m_surroundBetterThanAverage;
                if (surround < cutoff)
                  continue;
              }
              zs.RegisterLocation(location, randomPointInZone, false);
              placed++;
              break;
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
          if (errorAltBiomeMissing > 0)
            Print("Failed " + errorAltBiomeMissing + " times because the required alt biome was missing.");
          if (errorAltBiomeBlock > 0)
            Print("Failed " + errorAltBiomeBlock + " times because the alt biome blocked the location.");
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
