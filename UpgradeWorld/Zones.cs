using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;

// First step of the upgrader: Get an array of zones determined by the upgrade command.
public static class Zones
{
  public static Heightmap GetHeightmap(GameObject root)
  {
    return root.GetComponentInChildren<Heightmap>();
  }

  private static Vector2i[] Sort(IEnumerable<Vector2i> zones)
  {
    // Magnitude doesn't work with int.MinValue, so needs special handling.
    return [.. zones.OrderBy(zone => zone.x == int.MinValue || zone.y == int.MinValue ? int.MinValue : zone.Magnitude())];
  }
  public static Vector2i[] GetZones(FiltererParameters args)
  {
    var zs = ZoneSystem.instance;
    if (args.Zone.HasValue && args.MaxDistance == 0f && args.MinDistance == 0f)
    {
      if (args.TargetZones == TargetZones.Generated && !zs.m_generatedZones.Contains(args.Zone.Value))
        return [];
      if (args.TargetZones == TargetZones.Ungenerated && zs.m_generatedZones.Contains(args.Zone.Value))
        return [];
      return [args.Zone.Value];
    }
    return GetZones(args.TargetZones);
  }
  private static Vector2i[] GetZones(TargetZones zones)
  {
    if (zones == TargetZones.All) return GetAllZones();
    var zs = ZoneSystem.instance;
    if (zones == TargetZones.Generated) return Sort(zs.m_generatedZones);
    return GetAllZones().Where(zone => !zs.m_generatedZones.Contains(zone)).ToArray();
  }
  private static Vector2i[]? AllZones;
  public static Vector2i[] GetAllZones() => AllZones ??= GetWorldZones();
  public static void ResetAllZones() => AllZones = null;
  // Returns an array of all zones.
  private static Vector2i[] GetWorldZones()
  {
    var zs = ZoneSystem.instance;
    var zones = zs.m_generatedZones.ToHashSet();
    var limit = (int)Math.Ceiling(Settings.WorldRadius / zs.m_zoneSize);
    for (var i = -limit; i <= limit; i++)
    {
      for (var j = -limit; j <= limit; j++)
      {
        if (i * i + j * j > limit * limit) continue;
        zones.Add(new(i, j));
      }
    }
    return Sort(zones);
  }

  public static int Distance(Vector2i a, Vector2i b) => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
  public static bool IsWithin(Vector2i a, Vector2i b, int min, int max)
  {
    var distance = Distance(a, b);
    if (max == 0 && min > 0) max = int.MaxValue;
    return distance >= min && distance <= max;
  }
}
