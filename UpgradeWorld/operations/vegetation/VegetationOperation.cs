using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public abstract class VegetationOperation : ZoneOperation {
  public bool[] Original = null;
  public bool[] New = null;
  public VegetationOperation(Terminal context, FiltererParameters args) : base(context, args.Start) {
    args.TargetZones = TargetZones.Generated;
    Original = GetCurrent();
  }
  protected override bool OnExecute() {
    Set(New);
    return base.OnExecute();
  }
  protected override void OnEnd() {
    Set(Original);
  }
  public static bool[] GetCurrent() =>
    ZoneSystem.instance.m_vegetation.Select(veg => veg.m_enable).ToArray();
  public static bool[] GetWithIds(HashSet<string> ids, bool value) =>
    ZoneSystem.instance.m_vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : veg.m_enable).ToArray();
  public static bool[] GetWithOnlyIds(HashSet<string> ids, bool value) =>
    ZoneSystem.instance.m_vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : !value).ToArray();
  public static void Set(bool[] values) {
    var vegs = ZoneSystem.instance.m_vegetation;
    for (var i = 0; i < vegs.Count; i++) vegs[i].m_enable = values[i];
  }
}
