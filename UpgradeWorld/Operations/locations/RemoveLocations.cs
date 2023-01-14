using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Destroys given locations.</summary>
public class RemoveLocations : ExecutedOperation
{
  HashSet<string> Ids;
  FiltererParameters Args;
  public RemoveLocations(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context, args.Start)
  {
    Args = new(args);
    Args.TargetZones = TargetZones.All;
    Ids = ids.ToHashSet();
  }
  private int LocationProxyHash = "LocationProxy".GetStableHashCode();
  private int LocationHash = "location".GetStableHashCode();

  private int RemoveSpawned()
  {
    var zs = ZoneSystem.instance;
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => LocationProxyHash == zdo.GetPrefab());
    zdos = Args.FilterZdos(zdos);
    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (!Args.Roll()) continue;
      var zone = zs.GetZone(zdo.GetPosition());
      var name = "???";
      if (zs.m_locationsByHash.TryGetValue(zdo.GetInt(LocationHash), out var location))
      {
        name = location.m_prefabName;
        if (Ids.Count > 0 && !Ids.Contains(name)) continue;
        Helper.ClearZDOsWithinDistance(zone, zdo.GetPosition(), Args.ObjectReset != 0 ? location.m_location.m_exteriorRadius : Args.ObjectReset);
      }
      Helper.RemoveZDO(zdo);
      removed++;
      zs.m_locationInstances.Remove(zone);
      if (Settings.Verbose)
        Print($"Location {name} removed at {zone.ToString()}.");
    }
    var toRemove = zs.m_locationInstances.Where(kvp => Args.FilterPosition(kvp.Value.m_position)).ToList();
    foreach (var kvp in toRemove)
    {
      if (!Args.Roll()) continue;
      var zone = kvp.Key;
      var location = kvp.Value.m_location;
      var name = location.m_prefabName;
      if (Ids.Count > 0 && !Ids.Contains(name)) continue;
      Helper.ClearZDOsWithinDistance(zone, kvp.Value.m_position, Args.ObjectReset != 0 ? location.m_location.m_exteriorRadius : Args.ObjectReset);
      ResetTerrain.ResetRadius = Args.TerrainReset == 0f ? location.m_exteriorRadius : Args.TerrainReset;
      ResetTerrain.Execute(kvp.Value.m_position);
      removed++;
      zs.m_locationInstances.Remove(zone);
      if (Settings.Verbose)
        Print($"Location {name} removed at {zone.ToString()}.");
      ResetTerrain.ResetRadius = 0f;
    }
    return removed;

  }
  private int RemoveNotSpawned()
  {
    var zs = ZoneSystem.instance;
    var filterers = FiltererFactory.Create(Args);
    if (Ids.Count > 0)
      filterers = filterers.Append(new LocationFilterer(Ids)).ToList();
    List<string> messages = new();
    var notSpawnedZones = filterers.Aggregate(Zones.GetZones(TargetZones.All), (zones, filterer) => filterer.FilterZones(zones, ref messages));
    var removed = 0;
    foreach (var zone in notSpawnedZones)
    {
      if (!Args.Roll()) continue;
      if (!zs.m_locationInstances.TryGetValue(zone, out var location)) continue;
      removed++;
      if (Settings.Verbose)
        Print("Location " + location.m_location.m_prefabName + " removed at " + zone.ToString());
      zs.m_locationInstances.Remove(zone);
    }
    return removed;
  }

  protected override bool OnExecute()
  {
    var removed = RemoveSpawned() + RemoveNotSpawned();
    Print($"Removed {removed} locations.");
    return true;
  }

  protected override string OnInit()
  {
    return Args.Print($"Remove locations{Helper.LocationIdString(Ids)} from");
  }
}
