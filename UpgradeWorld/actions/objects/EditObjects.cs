using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class EditObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : ExecutedEntityOperation(context, ids, args)
{
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
    }
    return result;
  }
  protected override bool ProcessZDO(ZDO zdo) => SetData(zdo, Args.Datas);

  protected override string GetNoObjectsMessage() => "No objects found to update.";

  protected override string GetInitMessage() => $"Updating {TotalCount} object{(TotalCount > 1 ? "s" : "")}.";

  protected override string GetProcessedMessage() => $"Updated: {ProcessedCount}";

  protected override string GetCountMessage(int count, int prefab) => $"Updated {count} of {EntityOperation.GetName(prefab)}.";
}
