using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given distance.</summary>
  public class Remove : BaseOperation {
    private IEnumerable<string> Ids;
    private float Distance;
    public Remove(IEnumerable<string> ids, float distance, Terminal context) : base(context) {
      Ids = ids;
      Distance = distance;
    }
    protected override bool OnExecute() {
      foreach (var id in Ids) {
        var prefab = ZNetScene.instance.GetPrefab(id);
        if (prefab == null)
          Print("Error: Invalid entity ID " + id + ".");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var position = Player.m_localPlayer.transform.position;
          var toRemove = Distance > 0 ? zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < Distance) : zdos;
          foreach (var zdo in toRemove) ZDOMan.instance.DestroyZDO(zdo);
          Print("Removed " + toRemove.Count() + " of " + id + ".");
        }
      }
      return true;
    }
  }
}