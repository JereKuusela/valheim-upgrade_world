using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveObjects : EntityOperation {
  public RemoveObjects(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context) {
    Remove(ids, args);
  }
  private void Remove(IEnumerable<string> ids, DataParameters args) {
    var prefabs = ids.SelectMany(GetPrefabs);
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
