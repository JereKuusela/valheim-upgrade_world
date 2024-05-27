using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveObjects : EntityOperation
{
  public RemoveObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context, args.Pin)
  {
    Remove(ids, args);
  }
  private void Remove(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = GetPrefabs(ids, args.Types);
    var zdos = GetZDOs(args, prefabs);
    var total = 0;
    var counts = prefabs.ToDictionary(prefab => prefab, prefab => 0);
    foreach (var zdo in zdos)
    {
      if (!args.Roll()) continue;
      counts[zdo.m_prefab] += 1;
      total += 1;
      AddPin(zdo.GetPosition());
      Helper.RemoveZDO(zdo);
    }
    var linq = counts.Where(kvp => kvp.Value > 0).Select(kvp => $"Removed {kvp.Value} of {GetName(kvp.Key)}.");
    string[] texts = [$"Removed: {total}", .. linq];
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
