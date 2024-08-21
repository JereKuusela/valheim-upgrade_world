using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class EditObjects : EntityOperation
{
  public EditObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context, args.Pin)
  {
    Execute(ids, args);
  }
  private bool SetData(ZDO zdo, List<string> datas)
  {
    var revision = zdo.DataRevision;
    var result = datas.Where(data =>
    {
      var split = Parse.Split(data);
      var value = split.Length > 1 ? split[1] : "";
      var type = split.Length > 2 ? split[2] : "";
      return DataHelper.SetData(zdo, split[0], value, type);
    }).Count() > 0;
    if (result)
    {
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.DataRevision = revision + 1;
      AddPin(zdo.GetPosition());
    }
    return result;
  }
  private void Execute(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = GetPrefabs(ids, args.Types);
    var zdos = GetZDOs(args, prefabs);
    var total = 0;
    var counts = prefabs.ToDictionary(prefab => prefab, prefab => 0);
    foreach (var zdo in zdos)
    {
      if (!SetData(zdo, args.Datas))
        continue;
      counts[zdo.m_prefab] += 1;
      total += 1;
    }
    var linq = counts.Where(kvp => kvp.Value > 0).Select(kvp => $"Updated {kvp.Value} of {GetName(kvp.Key)}.");
    string[] texts = [$"Updated: {total}", .. linq];
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
