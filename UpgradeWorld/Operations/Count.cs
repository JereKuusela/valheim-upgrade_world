using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Counts the amounts of given entity ids within a given dostance.</summary>
  public class Count : BaseOperation {
    private IEnumerable<string> Ids;
    private float Distance;
    public Count(IEnumerable<string> ids, float distance, Terminal context) : base(context) {
      Ids = ids;
      Distance = distance;
    }
    private void CountAll() {
      var counts = new Dictionary<int, int>();
      var position = Player.m_localPlayer.transform.position;
      foreach (var zdo in ZDOMan.instance.m_objectsByID.Values) {
        if (Utils.DistanceXZ(zdo.GetPosition(), position) < Distance) continue;
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
          Print("Error: Invalid entity ID " + id + ".");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var position = Player.m_localPlayer.transform.position;
          var count = zdos.Count();
          if (Distance > 0) count = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < Distance).Count();
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