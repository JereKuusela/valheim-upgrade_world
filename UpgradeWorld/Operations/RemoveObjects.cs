using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveObjects : EntityOperation {
  public RemoveObjects(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
    if (Validate(ids))
      Remove(ids, args);
  }
  private void Remove(IEnumerable<string> ids, FiltererParameters args) {
    var prefabs = ids.Select(GetPrefabs).Aggregate((acc, list) => acc.Concat(list));
    var texts = prefabs.Select(id => {
      var zdos = GetZDOs(id, args);
      var removed = 0;
      foreach (var zdo in zdos) {
        if (!args.Roll()) continue;
        removed++;
        Helper.RemoveZDO(zdo);
      }
      return "Removed " + removed + " of " + id + ".";
    });
    Log(texts);
  }
}
