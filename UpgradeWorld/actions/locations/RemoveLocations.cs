using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys given locations.</summary>
public class RemoveLocations : ExecutedOperation
{
  readonly HashSet<string> Ids;
  readonly FiltererParameters Args;
  public RemoveLocations(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args.Start)
  {
    Args = new(args)
    {
      TargetZones = TargetZones.All
    };
    Ids = ids;
  }
  protected override void OnStart()
  {
    if (Args.SafeZones == 0) return;
    PlayerBaseFilterer filterer = new(Args.SafeZones);
    filterer.CalculateExcluded();
  }
  private int RemoveSpawned()
  {
    var zs = ZoneSystem.instance;
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => LocationProxyHash == zdo.m_prefab);
    zdos = Args.FilterZdos(zdos, true);
    var removed = 0;
    foreach (var zdo in zdos)
    {
      var zone = ZoneSystem.GetZone(zdo.GetPosition());
      var name = "???";
      if (zs.m_locationsByHash.TryGetValue(zdo.GetInt(LocationHash), out var location))
      {
        name = location.m_prefab.Name;
        if (!Ids.Contains(name)) continue;
        var clearRadius = location.m_exteriorRadius;
        if (Args.ObjectReset.HasValue) clearRadius = Args.ObjectReset.Value;
        AddPin(zdo.GetPosition());
        Helper.ClearZDOsWithinDistance(zone, zdo.GetPosition(), clearRadius);
      }
      Helper.RemoveZDO(zdo);
      removed++;
      zs.m_locationInstances.Remove(zone);
      if (Settings.Verbose)
        Print($"Location {name} removed at {zone}.");
    }
    var toRemove = zs.m_locationInstances.Where(kvp => Args.FilterPosition(kvp.Value.m_position, true)).ToList();
    foreach (var kvp in toRemove)
    {
      if (!Args.Roll()) continue;
      var zone = kvp.Key;
      var location = kvp.Value.m_location;
      var name = location.m_prefab.Name;
      if (Ids.Count > 0 && !Ids.Contains(name)) continue;
      var clearRadius = location.m_exteriorRadius;
      if (Args.ObjectReset.HasValue) clearRadius = Args.ObjectReset.Value;
      Helper.ClearZDOsWithinDistance(zone, kvp.Value.m_position, clearRadius);
      AddPin(kvp.Value.m_position);
      var resetRadius = Args.TerrainReset == 0f ? location.m_exteriorRadius : Args.TerrainReset;
      ResetTerrain.Execute(kvp.Value.m_position, resetRadius);
      removed++;
      zs.m_locationInstances.Remove(zone);
      if (Settings.Verbose)
        Print($"Location {name} removed at {zone}.");
    }
    return removed;

  }
  private int RemoveNotSpawned()
  {
    var zs = ZoneSystem.instance;
    var filterers = FiltererFactory.Create(Args);
    filterers = [.. filterers, new LocationFilterer(Ids, false)];
    List<string> messages = [];
    Args.TargetZones = TargetZones.All;
    var notSpawnedZones = filterers.Aggregate(Zones.GetZones(Args), (zones, filterer) => filterer.FilterZones(zones, ref messages));
    var removed = 0;
    foreach (var zone in notSpawnedZones)
    {
      if (!zs.m_locationInstances.TryGetValue(zone, out var location)) continue;
      removed++;
      AddPin(location.m_position);
      if (Settings.Verbose)
        Print("Location " + location.m_location.m_prefab.Name + " removed at " + zone.ToString());
      zs.m_locationInstances.Remove(zone);
    }
    return removed;
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    var removed = RemoveSpawned() + RemoveNotSpawned();
    Print($"Removed {removed} locations.");
    yield break;
  }

  protected override string OnInit()
  {
    return Args.Print($"Remove locations{LocationOperation.IdString(Ids)} from");
  }
}
