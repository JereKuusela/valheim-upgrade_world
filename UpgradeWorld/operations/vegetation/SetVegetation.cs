using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public class SetVegetation : BaseOperation {

  public static List<string> GetIds() {
    return ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).Distinct().ToList();
  }
  public SetVegetation(Terminal context, bool value, bool only, IEnumerable<string> ids) : base(context) {
    if (ResetVegetation.Original == null) ResetVegetation.Original = ZoneSystem.instance.m_vegetation.Select(veg => veg.m_enable).ToList();
    Set(ids, value, only);
  }
  private void Set(IEnumerable<string> ids, bool value, bool only) {
    var vegetation = ZoneSystem.instance.m_vegetation;
    foreach (var veg in vegetation) {
      if (ids.Contains(veg.m_prefab.name))
        veg.m_enable = value;
      else if (only)
        veg.m_enable = !value;
    }
  }
}
