using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Lists position and biome of each location.</summary>
public class ListLocationPositions : EntityOperation
{
  public ListLocationPositions(Terminal context, HashSet<string> ids, bool log, FiltererParameters args) : base(context, args.Pin)
  {
    ListPositions(ids, log, args);
  }
  private void ListPositions(HashSet<string> ids, bool log, FiltererParameters args)
  {
    var zs = ZoneSystem.instance;
    List<string> texts = [];
    var locs = args.FilterLocations(zs.m_locationInstances.Values).Where(l => ids.Count() == 0 || ids.Contains(l.m_location?.m_prefab.Name ?? "")).ToList();
    foreach (var loc in locs)
    {
      var location = loc.m_location;
      var name = location.m_prefab.Name;
      texts.Add($"{name}: {Helper.PrintVectorXZY(loc.m_position)} {WorldGenerator.instance.GetBiome(loc.m_position)}");
      AddPin(loc.m_position);
    }
    if (log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }

}
