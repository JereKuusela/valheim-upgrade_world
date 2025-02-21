namespace UpgradeWorld;
/// <summary>Removes missing objects from the world.</summary>
public class CleanObjects : EntityOperation
{
  public CleanObjects(Terminal context, ZDO[] zdos, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(zdos, alwaysPrint);
  }

  private void Clean(ZDO[] zdos, bool alwaysPrint)
  {
    var scene = ZNetScene.instance;

    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (scene.m_namedPrefabs.ContainsKey(zdo.m_prefab)) continue;
      AddPin(zdo.m_position);
      Helper.RemoveZDO(zdo);
      removed++;
      if (Settings.Verbose)
        Print($"Removed missing object {zdo.m_prefab} at {zdo.m_position}.");
    }
    if (alwaysPrint || removed > 0)
      Print($"Removed {removed} missing object{S(removed)}.");
  }

}
