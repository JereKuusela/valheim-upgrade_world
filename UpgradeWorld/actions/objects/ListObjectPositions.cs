using System.Collections.Generic;
using System.Linq;
using Service;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class ListObjectPositions : EntityOperation
{
  public ListObjectPositions(Terminal context, HashSet<string> ids, DataParameters args) : base(context, args.Pin)
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
  private void ListPositions(HashSet<string> ids, DataParameters args)
  {
    if (ids.Count > 0 && ids.All(id => int.TryParse(id, out _)))
    {
      var hashCodes = new HashSet<int>(ids.Select(int.Parse));
      ListPositions(hashCodes, args);
    }
    else
    {
      ListPositions(GetPrefabs(ids, args.Types), args);
    }
  }
  private void ListPositions(HashSet<int> ids, DataParameters args)
  {
    var zdos = GetZDOs(args, ids);
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
