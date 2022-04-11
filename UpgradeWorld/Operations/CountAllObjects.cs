using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Counts the amounts of entities.</summary>
public class CountAllObjects : EntityOperation {
  public CountAllObjects(Terminal context, bool showAll, FiltererParameters args) : base(context) {
    Count(showAll, args);
  }
  private void Count(bool showAll, FiltererParameters args) {
    Dictionary<int, int> counts = new();
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
      } else if (showAll) {
        return id + ": 0";
      }
      return "";
    }).Where(text => text != "");
    Log(texts);
  }
}
