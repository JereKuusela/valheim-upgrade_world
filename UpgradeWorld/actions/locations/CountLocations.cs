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
    PlayerBaseFilterer.Update(args.SafeZones);
    var total = 0;
    var totalSkipped = 0;
    var counts = new Dictionary<string, int>();
    var skippedCounts = new Dictionary<string, int>();
    foreach (var loc in locs)
    {
      var name = loc.m_location.m_prefab.Name;
      if (!counts.ContainsKey(name))
      {
        counts[name] = 0;
        skippedCounts[name] = 0;
      }
      if (PlayerBaseFilterer.IsExcluded(loc.m_position))
      {
        skippedCounts[name] += 1;
        totalSkipped += 1;
        continue;
      }
      counts[name] += 1;
      total += 1;
      AddPin(loc.m_position);
    }
    var linq = counts.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}: {kvp.Value}{SkippedText(skippedCounts[kvp.Key])}");
    List<string> texts = [$"Total: {total}{SkippedText(totalSkipped)}", .. linq];
    if (log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
  private static string SkippedText(int skipped) => skipped > 0 ? $" ({skipped} skipped by having a player base)" : "";
}
