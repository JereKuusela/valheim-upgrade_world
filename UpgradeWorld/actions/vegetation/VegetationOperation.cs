using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public abstract class VegetationOperation : ZoneOperation
{
  public static List<string> GetIds()
  {
    return [.. ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).Distinct()];
  }
  public HashSet<string> Ids = [];
  private static List<ZoneSystem.ZoneVegetation> OriginalVegetation = [];
  private static List<ZoneSystem.ZoneVegetation> Vegetation = [];
  public VegetationOperation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args)
  {
    args.TargetZones = TargetZones.Generated;
    Ids = ids;
  }
  protected override void OnStart()
  {
    base.OnStart();
    OriginalVegetation = ZoneSystem.instance.m_vegetation;
    Vegetation = [.. ZoneSystem.instance.m_vegetation.Select(veg => veg.Clone())];
    if (Ids.Count > 0)
      Set(GetWithOnlyIds(Ids, true));
  }
  protected void OverrideVegetation()
  {
    ZoneSystem.instance.m_vegetation = Vegetation;
  }
  protected void RestoreVegetation()
  {
    ZoneSystem.instance.m_vegetation = OriginalVegetation;
  }
  public static bool[] GetWithIds(HashSet<string> ids, bool value) =>
    [.. Vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : veg.m_enable)];
  public static bool[] GetWithOnlyIds(HashSet<string> ids, bool value) =>
    [.. Vegetation.Select(veg => ids.Contains(veg.m_prefab.name) ? value : !value)];
  public static void Set(bool[] values)
  {
    for (var i = 0; i < Vegetation.Count; i++) Vegetation[i].m_enable = values[i];
  }
}
