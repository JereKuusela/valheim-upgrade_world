using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public abstract class VegetationOperation : ZoneOperation {
  public bool[] Original = new bool[0];
  public HashSet<string> Ids = new();
  private List<ZoneSystem.ZoneVegetation> OriginalVegetation = new();
  public VegetationOperation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args.Start) {
    args.TargetZones = TargetZones.Generated;
    Ids = ids;
  }
  protected override void OnStart() {
    OriginalVegetation = ZoneSystem.instance.m_vegetation;
    if (VegetationData.Load())
      Helper.Print(Context, User, $"{ZoneSystem.instance.m_vegetation.Count} vegetations loaded from vegetation.json");
    Original = GetCurrent();
    Set(GetWithOnlyIds(Ids, true));
  }
  protected override void OnEnd() {
    Set(Original);
    ZoneSystem.instance.m_vegetation = OriginalVegetation;
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
