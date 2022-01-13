using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Removes given entity ids within a given distance.</summary>
  public class RemoveLocations : EntityOperation {
    public RemoveLocations(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
      if (Validate(ids))
        Remove(ids, args);
    }
    private void Remove(IEnumerable<string> ids, FiltererParameters args) {
      var prefabs = GetPrefabs("_LocationProxy");
      var locations = ids.Select(id => id.GetStableHashCode()).ToHashSet();
      var texts = prefabs.Select(id => {
        var zdos = GetZDOs(id, args).Where(zdo => locations.Contains(zdo.GetInt("location", 0)));
        foreach (var zdo in zdos) {
          Helper.RemoveZDO(zdo);
        }
        return "Removed " + zdos.Count() + " of " + id + ".";
      });
      if (Settings.Verbose)
        Log(texts);
    }
  }
}