using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Counts the amounts of given entities.</summary>
public class CountEntities : EntityOperation {
  public CountEntities(Terminal context, IEnumerable<string> ids, FiltererParameters args) : base(context) {
    if (Validate(ids))
      Count(ids, args);
  }
  private void Count(IEnumerable<string> ids, FiltererParameters args) {
    var prefabs = ids.Select(GetPrefabs).Aggregate((acc, list) => acc.Concat(list));
    var texts = prefabs.Select(id => {
      var count = GetZDOs(id, args).Count();
      return id + ": " + count;
    });
    Log(texts);
  }
}
