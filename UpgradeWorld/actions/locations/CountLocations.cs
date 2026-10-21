using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld;
/// <summary>Counts locations and returns totals for each location type.</summary>
public class CountLocations : EntityOperation
{
  public CountLocations(Terminal context, HashSet<string> ids, bool log, FiltererParameters args) : base(context, args.Pin)
  {
    CountPositions(ids, log, args);
  }
  private void CountPositions(HashSet<string> ids, bool log, FiltererParameters args)
  {
    var zs = ZoneSystem.instance;
    var locs = args.FilterLocations(zs.m_locationInstances.Values).Where(l => ids.Count() == 0 || ids.Contains(l.m_location?.m_prefab.Name ?? "")).ToList();
    var skipped = PlayerBaseFilterer.RemoveExcluded(locs, args.SafeZones);
    var total = 0;
    var counts = new Dictionary<string, int>();
    foreach (var loc in locs)
    {
      var location = loc.m_location;
      var name = location.m_prefab.Name;
      if (!counts.ContainsKey(name))
        counts[name] = 0;
      counts[name] += 1;
      total += 1;
      AddPin(loc.m_position);
    }
    var linq = counts.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}: {kvp.Value}");
    List<string> texts = [$"Total: {total}", .. linq];
    if (skipped > 0) texts.Add($"{skipped} locations skipped by having a player base");
    if (log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
