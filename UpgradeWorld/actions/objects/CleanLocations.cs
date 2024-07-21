using System.Linq;
using SoftReferenceableAssets;

namespace UpgradeWorld;
/// <summary>Removes missing location objects.</summary>
public class CleanLocations : EntityOperation
{
  public CleanLocations(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var scene = ZNetScene.instance;
    var zs = ZoneSystem.instance;
    var toRemove = zs.m_locationInstances.Where(x =>
    {
      var assetId = x.Value.m_location?.m_prefab.m_assetID;
      // Custom check so that blueprint locations for Expand World Data are not removed.
      // They use the empty asset ID because no asset is loaded. However the mod also makes the empty asset ID available.
      return assetId == null || !Runtime.Loader.IsAvailable(assetId.Value);
    }).Select(x => x.Key).ToList();
    foreach (var zone in toRemove)
      zs.m_locationInstances.Remove(zone);
    if (alwaysPrint || toRemove.Count > 0)
      Print($"Removed {toRemove.Count} missing location entries.");

    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (zdo.m_prefab != LocationProxyHash) continue;
      if (zs.GetLocation(zdo.GetInt(LocationHash)) != null) continue;
      Helper.RemoveZDO(zdo);
      removed++;
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing location object{S(removed)}.");
  }
}
