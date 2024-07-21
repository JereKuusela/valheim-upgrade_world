
namespace UpgradeWorld;
/// <summary>Removes timestamps from the spawn system.</summary>
public class CleanSpawns : EntityOperation
{
  public CleanSpawns(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var zs = ZoneSystem.instance;
    var prefab = zs.m_zoneCtrlPrefab;
    var zoneCtrlHash = prefab.name.GetStableHashCode();
    var reseted = 0;
    var count = 0;
    foreach (var zdo in zdos)
    {
      if (zdo.m_prefab != zoneCtrlHash) continue;
      var id = zdo.m_uid;
      var longs = ZDOExtraData.GetLongs(id);
      if (longs.Count < 1) continue;
      AddPin(zdo.m_position);
      count += longs.Count;
      zdo.SetOwner(ZDOMan.GetSessionID());
      ZDOHelper.Release(ZDOExtraData.s_longs, id);
      zdo.IncreaseDataRevision();
      reseted++;
    }
    if (alwaysPrint || count > 0)
      Print($"Cleared {count} spawn data from {reseted} zone control{S(reseted)}.");
  }
}
