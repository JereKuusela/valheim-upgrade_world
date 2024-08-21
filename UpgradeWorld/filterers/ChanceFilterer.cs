using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public class ChanceFilterer(float chance) : IZoneFilterer
{
  private static Random random = new();
  public float Chance = chance;

  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    var amount = zones.Length;
    zones = zones.Where(zone => random.NextDouble() < Chance).ToArray();
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by chance");
    return zones;
  }
}
