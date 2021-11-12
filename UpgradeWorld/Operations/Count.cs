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
    private void CountAll() {
      var counts = new Dictionary<int, int>();
      var position = Player.m_localPlayer.transform.position;
      foreach (var zdo in ZDOMan.instance.m_objectsByID.Values) {
        if (Utils.DistanceXZ(zdo.GetPosition(), position) < Radius) continue;
        var id = zdo.GetPrefab();
        if (!counts.ContainsKey(id)) counts[id] = 0;
        counts[id]++;
      }

      foreach (var id in ZNetScene.instance.GetPrefabNames()) {
        var code = id.GetStableHashCode();
        if (counts.ContainsKey(code))
          Print(id + ": " + counts[code]);
      }
    }
    private void CountGiven() {
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
    }
    protected override bool OnExecute() {
      if (Ids.Count() == 0)
        CountAll();
      else
        CountGiven();
      return true;
    }
  }
}