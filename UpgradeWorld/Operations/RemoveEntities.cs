using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given distance.</summary>
  public class RemoveEntities : BaseOperation {
    public RemoveEntities(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
      Remove(ids, args);
    }
    private void Remove(IEnumerable<string> ids, FiltererParameters args) {
      foreach (var id in ids) {
        var prefab = ZNetScene.instance.GetPrefab(id);
        if (prefab == null)
          Print("Error: Invalid entity ID " + id + ".");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var toRemove = args.FilterZdos(zdos);
          foreach (var zdo in toRemove) ZDOMan.instance.DestroyZDO(zdo);
          Print("Removed " + toRemove.Count() + " of " + id + ".");
        }
      }
    }
  }
}