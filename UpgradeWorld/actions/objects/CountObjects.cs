using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Counts the amounts of given entities.</summary>
public class CountObjects : EntityOperation
{
  public CountObjects(Terminal context, List<string> ids, DataParameters args, Range<int> countRange) : base(context)
  {
    Count(ids, args, countRange);
  }
  private void Count(List<string> ids, DataParameters args, Range<int> countRange)
  {
    if (ids.Count == 0) ids.Add("*");
    var prefabs = ids.SelectMany(GetPrefabs).ToHashSet();
    var total = 0;
    var zdos = GetZDOs(args);
    var texts = prefabs.Select(id =>
    {
      var count = GetZDOs(zdos, id).Count();
      if (count < countRange.Min || count >= countRange.Max) return "";
      total += count;
      return $"{id}: {count}";
    }).Where(s => s != "").ToArray();
    texts = texts.Prepend($"Total: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
