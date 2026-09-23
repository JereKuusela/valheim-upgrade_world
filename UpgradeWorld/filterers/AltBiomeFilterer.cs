using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;

///<summary>Placeholder for filtering zones based on given alternate biomes.</summary>
public class AltBiomeFilterer(IEnumerable<string> AltBiomes) : IZoneFilterer
{
  public Vector2s[] FilterZones(Vector2s[] zones, ref List<string> messages)
  {
    if (AltBiomes.Count() == 0) return zones;
    return zones.Where(zone => WorldGenerator.instance.GetBiomeSector(zone.x, zone.y).AltBiomes.Any(ab => AltBiomes.Contains(ab.m_name))).ToArray();
  }
}