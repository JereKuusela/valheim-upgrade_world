using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  public class ResetVegetation : BaseOperation {
    public static List<bool> Original = null;
    public ResetVegetation(Terminal context) : base(context) {
      if (Original == null) Original = ZoneSystem.instance.m_vegetation.Select(veg => veg.m_enable).ToList();
      Reset();
    }
    private void Reset() {
      var vegetation = ZoneSystem.instance.m_vegetation;
      for (var i = 0; i < vegetation.Count; i++)
        vegetation[i].m_enable = Original[i];
    }
  }
}