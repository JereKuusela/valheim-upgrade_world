using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Lists position and biome of each location.</summary>
public class ListLocationPositions : EntityOperation
{
  public ListLocationPositions(Terminal context, IEnumerable<string> ids, LocationIdParameters args) : base(context)
  {
    ListPositions(ids, args);
  }
  private void ListPositions(IEnumerable<string> ids, LocationIdParameters args)
  {
    var zs = ZoneSystem.instance;
    List<string> texts = new();
    var toPrint = args.FilterLocations(zs.m_locationInstances.Values).ToList();
    foreach (var loc in toPrint)
    {
      var location = loc.m_location;
      var name = location.m_prefabName;
      if (ids.Count() > 0 && !ids.Contains(name)) continue;
      texts.Add(name + ": " + loc.m_position.ToString("F0") + " " + WorldGenerator.instance.GetBiome(loc.m_position));
    }
    if (args.Log) Log(texts);
    else Print(texts, false);
  }
}
