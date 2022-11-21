using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public abstract class VegetationOperation : ZoneOperation
{
  public static List<string> GetIds()
  {
    return ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).Distinct().ToList();
  }
  public HashSet<string> Ids = new();
  private List<ZoneSystem.ZoneVegetation> OriginalVegetation = new();
  public VegetationOperation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args)
  {
    args.TargetZones = TargetZones.Generated;
    Ids = ids;
  }
  protected override void OnStart()
  {
    OriginalVegetation = ZoneSystem.instance.m_vegetation;
    ZoneSystem.instance.m_vegetation = ZoneSystem.instance.m_vegetation.Select(veg => veg.Clone()).ToList();
    if (Args.Chance != 1f) ApplyChance(Args.Chance);
    if (Ids.Count > 0)
      Set(GetWithOnlyIds(Ids, true));
  }
  protected override void OnEnd()
  {
    ZoneSystem.instance.m_vegetation = OriginalVegetation;
  }
  public static bool[] GetWithIds(HashSet<string> ids, bool value) =>
    ZoneSystem.instance.m_vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : veg.m_enable).ToArray();
  public static bool[] GetWithOnlyIds(HashSet<string> ids, bool value) =>
    ZoneSystem.instance.m_vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : !value).ToArray();
  public static void Set(bool[] values)
  {
    var vegs = ZoneSystem.instance.m_vegetation;
    for (var i = 0; i < vegs.Count; i++) vegs[i].m_enable = values[i];
  }
  public static void ApplyChance(float chance)
  {
    var vegs = ZoneSystem.instance.m_vegetation;
    for (var i = 0; i < vegs.Count; i++)
    {
      vegs[i].m_min *= chance;
      vegs[i].m_max *= chance;
    }
  }


}
