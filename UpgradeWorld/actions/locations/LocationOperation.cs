using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;

public abstract class LocationOperation : ZoneOperation
{
  protected int Operated = 0;
  protected string Verb = "";
  public LocationOperation(Terminal context, FiltererParameters args) : base(context, args)
  {
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var locations = zs.m_locationInstances;
    if (!locations.TryGetValue(zone, out var location)) return true;
    if (zs.IsZoneLoaded(zone))
    {
      if (ExecuteLocation(zone, location)) Operated++;
      Zones.ReleaseZone(zone);
      return true;
    }
    Zones.PokeZone(zone);
    return false;
  }
  protected abstract bool ExecuteLocation(Vector2i zone, ZoneSystem.LocationInstance location);
  protected override void OnEnd()
  {
    var text = $"{Operation} completed. {Operated} locations {Verb}.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
  /// <summary>Spawns a location to the game world.</summary>
  protected void SpawnLocation(Vector2i zone, ZoneSystem.LocationInstance location, float clearRadius)
  {
    var zs = ZoneSystem.instance;
    var root = zs.m_zones[zone].m_root;
    var zonePos = ZoneSystem.GetZonePos(zone);
    var heightmap = Zones.GetHeightmap(root);
    Helper.ClearAreaForLocation(zone, location, clearRadius);
    var resetRadius = Args.TerrainReset == 0f ? location.m_location.m_exteriorRadius : Args.TerrainReset;
    ResetTerrain.Execute(location.m_position, resetRadius);
    zs.m_tempSpawnedObjects.Clear();
    zs.m_tempClearAreas.Clear();
    zs.PlaceLocations(zone, zonePos, root.transform, heightmap, zs.m_tempClearAreas, ZoneSystem.SpawnMode.Ghost, zs.m_tempSpawnedObjects);
    foreach (var obj in zs.m_tempSpawnedObjects)
      UnityEngine.Object.Destroy(obj);
    zs.m_tempSpawnedObjects.Clear();
  }
  public static HashSet<string> Ids(IEnumerable<string> ids, IEnumerable<string> ignore)
  {
    // Safeguard against missing the argument.
    if (ids.Count() == 0) return [];
    return [.. AllIds().Where(id => ids.Any(x => Helper.IsIncluded(x, id)) && (ignore.Count() == 0 || !ignore.Contains(id)))];
  }
  public static List<string> AllIds() => [.. ZoneSystem.instance.m_locations.Where(Helper.IsValid).Select(location => location.m_prefab.Name).Distinct()];

  public static string IdString(IEnumerable<string> ids)
  {
    if (ids.Count() == AllIds().Count) return "";
    if (ids.Count() == 0) return "";
    return " " + Helper.JoinRows(ids);
  }
  public static void Register(string name)
  {
    CommandWrapper.Register(name, AutoComplete, Named);
  }
  private static Dictionary<string, Func<int, List<string>?>> CreateNamed()
  {
    var named = FiltererParameters.GetAutoComplete();
    named["id"] = index => AllIds();
    named["ignore"] = index => AllIds();
    return named;
  }

  private static readonly Dictionary<string, Func<int, List<string>?>> Named = CreateNamed();

  private static readonly List<string> Parameters = [.. FiltererParameters.Parameters.Concat(["id", "ignore"]).OrderBy(x => x)];
  private static readonly Func<int, List<string>> AutoComplete = index => index == 0 ? AllIds() : Parameters;
}
