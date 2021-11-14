using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Counts the amounts of given entity ids within a given dostance.</summary>
  public class Count : BaseOperation {
    public Count(IEnumerable<string> ids, float distance, Terminal context) : base(context) {
      OnInit(ids, distance);
    }
    private void CountAll(float distance) {
      var counts = new Dictionary<int, int>();
      var position = Player.m_localPlayer.transform.position;
      foreach (var zdo in ZDOMan.instance.m_objectsByID.Values) {
        if (Utils.DistanceXZ(zdo.GetPosition(), position) < distance) continue;
        var id = zdo.GetPrefab();
        if (!counts.ContainsKey(id)) counts[id] = 0;
        counts[id]++;
      }
      var toLog = new List<string>();
      foreach (var id in ZNetScene.instance.GetPrefabNames()) {
        var code = id.GetStableHashCode();
        if (counts.ContainsKey(code)) {
          var text = id + ": " + counts[code];
          toLog.Add(text);
          Print(text);
        }
      }

      ZLog.Log(string.Join("\n", toLog));
    }
    private void CountGiven(IEnumerable<string> ids, float distance) {
      foreach (var id in ids) {
        var prefab = ZNetScene.instance.GetPrefab(id);
        if (prefab == null)
          Print("Error: Invalid entity ID " + id + ".");
        else {
          var zdos = new List<ZDO>();
          ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
          var position = Player.m_localPlayer.transform.position;
          var count = zdos.Count();
          if (distance > 0) count = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < distance).Count();
          Print(id + ": " + count);
        }
      }
    }
    private void OnInit(IEnumerable<string> ids, float distance) {
      if (ids.Count() == 0)
        CountAll(distance);
      else
        CountGiven(ids, distance);
    }
  }
}