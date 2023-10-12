using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class EditObjects : EntityOperation
{
  public EditObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
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
    var prefabs = ids.SelectMany(GetPrefabs).ToList();
    var total = 0;
    var zdos = GetZDOs(args);
    var texts = prefabs.Select(id =>
    {
      var updated = GetZDOs(zdos, id).Where(zdo => SetData(zdo, args.Datas)).Count();
      total += updated;
      return "Updated " + updated + " of " + id + ".";
    }).ToArray();
    texts = texts.Prepend($"Updated: {total}").ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
