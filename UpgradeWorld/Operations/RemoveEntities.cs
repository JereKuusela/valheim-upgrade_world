using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given distance.</summary>
  public class RemoveEntities : EntityOperation {
    public RemoveEntities(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
      if (Validate(ids))
        Remove(ids, args);
    }
    private void Remove(IEnumerable<string> ids, FiltererParameters args) {
      var texts = ids.Select(id => {
        var zdos = GetZDOs(id, args);
        foreach (var zdo in zdos) ZDOMan.instance.DestroyZDO(zdo);
        return "Removed " + zdos.Count() + " of " + id + ".";
      });
      Log(texts);
    }
  }
}