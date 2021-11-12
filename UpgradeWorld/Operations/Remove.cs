using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given radius.</summary>
  public class Remove : BaseOperation {
    private IEnumerable<string> Ids;
    private float Radius;
    public Remove(IEnumerable<string> ids, float radius, Terminal context) : base(context) {
      Ids = ids;
      Radius = radius;
    }
    protected override bool OnExecute() {
      foreach (var id in Ids) {
        var prefab = ZNetScene.instance.GetPrefab(id);
        if (prefab == null)
          Print(id + ": Invalid entity ID.");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var position = Player.m_localPlayer.transform.position;
          var toRemove = Radius > 0 ? zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < Radius) : zdos;
          foreach (var zdo in toRemove) ZDOMan.instance.DestroyZDO(zdo);
          Print("Removed " + id + ": " + toRemove.Count());
        }
      }
      return true;
    }
  }
}