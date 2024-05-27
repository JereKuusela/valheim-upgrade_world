using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class ListObjectPositions : EntityOperation
{
  public ListObjectPositions(Terminal context, List<string> ids, DataParameters args) : base(context, args.Pin)
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
      return DataHelper.GetData(zdo, split[0], type);
    }).ToList());
  }
  private void ListPositions(List<string> ids, DataParameters args)
  {
    var zdos = GetZDOs(args, GetPrefabs(ids, args.Types));
    var texts = zdos.Select(zdo =>
    {
      AddPin(zdo.GetPosition());
      return $"{GetName(zdo.m_prefab)} {zdo.m_uid.ID} {Helper.PrintVectorXZY(zdo.GetPosition())}: {GetData(zdo, args.Prints)}";
    }).ToArray();
    if (args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }
}
