using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Filters zones based on whether they include a player base item.</summary>
public class PlayerBaseFilterer : ZoneFilterer
{
  public HashSet<Vector2i> ExcludedZones = new();
  public PlayerBaseFilterer(int size)
  {
    var adjacent = size - 1;
    var ids = Settings.SafeZoneItems;
    var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => ids.Contains(zdo.GetPrefab()) && zdo.GetLong("creator") != 0L);
    foreach (var zdo in zdos)
    {
      var zone = ZoneSystem.instance.GetZone(zdo.GetPosition());
      for (var i = -adjacent; i <= adjacent; i++)
      {
        for (var j = -adjacent; j <= adjacent; j++)
        {
          ExcludedZones.Add(new(zone.x + i, zone.y + j));
        }
      }
    }
  }
  public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages)
  {
    var amount = zones.Length;
    zones = zones.Where(zone => !ExcludedZones.Contains(zone)).ToArray();
    var skipped = amount - zones.Length;
    if (skipped > 0) messages.Add(skipped + " skipped by having a player base");
    return zones;
  }
}
