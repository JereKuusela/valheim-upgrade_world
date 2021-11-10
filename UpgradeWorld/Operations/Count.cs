using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Counts the amounts of given entity ids within a given radius.</summary>
  public class Count : BaseOperation {
    private IEnumerable<string> Ids;
    private float Radius;
    public Count(IEnumerable<string> ids, float radius, Terminal context) : base(context) {
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
          var count = zdos.Count();
          if (Radius > 0) count = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < Radius).Count();
          Print(id + ": " + count);
        }
      }
      return true;
    }
  }
}