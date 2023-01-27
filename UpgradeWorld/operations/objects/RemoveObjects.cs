using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveObjects : EntityOperation
{
  public RemoveObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
  {
    Remove(ids, args);
  }
  private void Remove(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = ids.SelectMany(GetPrefabs).ToList();
    var total = 0;
    var texts = prefabs.Select(id =>
    {
      var zdos = GetZDOs(id, args);
      var removed = 0;
      foreach (var zdo in zdos)
      {
        if (!args.Roll()) continue;
        removed++;
        Helper.RemoveZDO(zdo);
      }
      total += removed;
      return "Removed " + removed + " of " + id + ".";
    }).ToArray();
    texts = texts.Prepend($"Removed: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
  }
}
