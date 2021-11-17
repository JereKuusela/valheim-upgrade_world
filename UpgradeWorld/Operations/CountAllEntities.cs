using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Counts the amounts of entities.</summary>
  public class CountAllEntities : EntityOperation {
    public CountAllEntities(Terminal context, bool showZero, FiltererParameters args) : base(context) {
      Count(showZero, args);
    }
    private void Count(bool showZero, FiltererParameters args) {
      var counts = new Dictionary<int, int>();
      var zdos = FilterZdos(ZDOMan.instance.m_objectsByID.Values, args);
      foreach (var zdo in zdos) {
        var id = zdo.GetPrefab();
        if (!counts.ContainsKey(id)) counts[id] = 0;
        counts[id]++;
      }
      var texts = ZNetScene.instance.GetPrefabNames().Select(id => {
        var code = id.GetStableHashCode();
        if (counts.ContainsKey(code)) {
          return id + ": " + counts[code];
        } else if (showZero) {
          return id + ": 0";
        }
        return "";
      }).Where(text => text != "");
      Log(texts);
    }
  }
}