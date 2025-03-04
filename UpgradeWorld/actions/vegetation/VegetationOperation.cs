using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public abstract class VegetationOperation : ZoneOperation
{
  public static List<string> AllIds()
  {
    return [.. ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).Distinct()];
  }
  public static HashSet<string> GetIds(List<string> ids, List<string> ignore)
  {
    return [.. AllIds().Where(id => (ids.Count == 0 || ids.Contains(id)) && (ignore.Count == 0 || !ignore.Contains(id)))];
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

  public static void Register(string name)
  {
    CommandWrapper.Register(name, AutoComplete, Named);
  }
  private static Dictionary<string, Func<int, List<string>?>> CreateNamed()
  {
    var named = FiltererParameters.GetAutoComplete();
    named["id"] = index => AllIds();
    named["ignore"] = index => AllIds();
    return named;
  }

  private static readonly Dictionary<string, Func<int, List<string>?>> Named = CreateNamed();

  private static readonly List<string> Parameters = [.. FiltererParameters.Parameters.Concat(["id", "ignore"]).OrderBy(x => x)];
  private static readonly Func<int, List<string>> AutoComplete = index => index == 0 ? AllIds() : Parameters;
}
