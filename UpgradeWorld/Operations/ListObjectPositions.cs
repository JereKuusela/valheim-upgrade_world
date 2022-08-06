using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Lists positon and biome of each entity.</summary>
public class ListObjectPositions : EntityOperation {
  public ListObjectPositions(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context) {
    if (Validate(ids))
      ListPositions(ids, args);
  }
  private void ListPositions(IEnumerable<string> ids, DataParameters args) {
    var texts = ids.Select(id => {
      return GetZDOs(id, args).Select(zdo => id + " (" + zdo.m_uid + "): " + zdo.GetPosition().ToString("F0") + " " + WorldGenerator.instance.GetBiome(zdo.GetPosition()));
    }).Aggregate((acc, list) => acc.Concat(list));
    Log(texts);
  }
}
