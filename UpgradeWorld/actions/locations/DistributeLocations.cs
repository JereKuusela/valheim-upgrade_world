using System;
using System.Collections.Generic;
using System.Linq;

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
  public DistributeLocations(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args.Start)
  {
    Ids = [.. ids];
    if (Ids.Length == 0)
      Ids = ZoneSystem.instance.m_locations.OrderByDescending(loc => loc.m_prioritized).Where(loc => loc.m_enable && loc.m_quantity != 0).Select(loc => loc.m_prefab.Name).ToArray();
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
      Print($"Generating locations {Ids[Attempts]}. This may take a while...");
      return false;
    }
    if (Attempts <= Ids.Length)
    {
      SpawnToAlreadyGenerated = true;
      var zs = ZoneSystem.instance;
      var id = Ids[Attempts - 1];
      var locations = zs.m_locations.Where(location => location.m_prefab.Name == id).ToArray();
      if (locations.Length == 0) return false;
      // Note: Printing is done one step before the execution, otherwise it would get printed afterwards.
      if (Attempts < Ids.Length)
        Print($"Generating locations {Ids[Attempts]}. This may take a while...");
      var before = zs.m_locationInstances.Count;
      ClearNotSpawned(id);
      foreach (var location in locations)
        zs.GenerateLocations(location);
      if (Chance < 1f)
      {
        zs.m_locationInstances = zs.m_locationInstances
          .Where(kvp => kvp.Value.m_placed || kvp.Value.m_location.m_prefab.Name != id || FiltererParameters.random.NextDouble() < Chance)
          .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      }
      var unplaced = zs.m_locationInstances.Where(kvp => kvp.Value.m_location.m_prefab.Name == id && !kvp.Value.m_placed).ToList();
      foreach (var kvp in unplaced) AddPin(kvp.Value.m_position);
      Total += Count(id);
      Added += zs.m_locationInstances.Count - before;
      SpawnToAlreadyGenerated = false;
      return false;
    }
    return true;
  }
  private void ClearNotSpawned(string id)
  {
    var zs = ZoneSystem.instance;
    zs.m_locationInstances = zs.m_locationInstances.Where(kvp => kvp.Value.m_placed || kvp.Value.m_location.m_prefab.Name != id).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
}
