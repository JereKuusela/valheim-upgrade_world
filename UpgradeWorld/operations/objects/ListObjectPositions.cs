using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class ListObjectPositions : EntityOperation
{
  public ListObjectPositions(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context)
  {
    ListPositions(ids, args);
  }
  private string GetData(ZDO zdo, List<string> prints)
  {
    if (prints.Count == 0) return WorldGenerator.instance.GetBiome(zdo.GetPosition()).ToString();
    return string.Join(" | ", prints.Select(print =>
    {
      var split = Parse.Split(print);
      var type = split.Length > 1 ? split[1] : "";
      return DataHelper.GetData(zdo, split[0].GetStableHashCode(), type);
    }).ToList());
  }
  private void ListPositions(IEnumerable<string> ids, DataParameters args)
  {
    var texts = ids.Select(id =>
    {
      return GetZDOs(id, args).Select(zdo => id + " " + zdo.m_uid.m_id + " " + Helper.PrintVectorXZY(zdo.GetPosition()) + ": " + GetData(zdo, args.Prints));
    }).Aggregate((acc, list) => acc.Concat(list));
    if (args.Log) Log(texts);
    else Print(texts, false);
  }
}
