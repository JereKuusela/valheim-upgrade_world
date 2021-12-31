using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Lists positon and biome of each entity.</summary>
  public class ListEntityPositions : EntityOperation {
    public ListEntityPositions(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
      if (Validate(ids))
        ListPositions(ids, args);
    }
    private void ListPositions(IEnumerable<string> ids, FiltererParameters args) {
      var texts = ids.Select(id => {
        return GetZDOs(id, args).Select(zdo => id + " (" + zdo.m_uid + "): " + zdo.GetPosition().ToString("F0") + " " + WorldGenerator.instance.GetBiome(zdo.GetPosition()));
      }).Aggregate((acc, list) => acc.Concat(list));
      Log(texts);
    }
  }
}