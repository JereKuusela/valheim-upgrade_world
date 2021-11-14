using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given distance.</summary>
  public class RemoveEntities : BaseOperation {
    public RemoveEntities(IEnumerable<string> ids, float distance, Terminal context) : base(context) {
      Remove(ids, distance);
    }
    private void Remove(IEnumerable<string> ids, float distance) {
      foreach (var id in ids) {
        var prefab = ZNetScene.instance.GetPrefab(id);
        if (prefab == null)
          Print("Error: Invalid entity ID " + id + ".");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var position = Player.m_localPlayer.transform.position;
          var toRemove = distance > 0 ? zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < distance) : zdos;
          foreach (var zdo in toRemove) ZDOMan.instance.DestroyZDO(zdo);
          Print("Removed " + toRemove.Count() + " of " + id + ".");
        }
      }
    }
  }
}