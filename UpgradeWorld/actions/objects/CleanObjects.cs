namespace UpgradeWorld;
/// <summary>Removes missing objects from the world.</summary>
public class CleanObjects : EntityOperation
{
  public CleanObjects(Terminal context, ZDO[] zdos, bool pin) : base(context, pin)
  {
    Clean(zdos);
  }

  private void Clean(ZDO[] zdos)
  {
    var scene = ZNetScene.instance;

    var removed = 0;
    foreach (var zdo in zdos)
    {
      if (scene.m_namedPrefabs.ContainsKey(zdo.m_prefab)) continue;
      AddPin(zdo.m_position);
      Helper.RemoveZDO(zdo);
      removed++;
    }
    if (removed > 0)
      Print($"Removed {removed} missing object{S(removed)}.");
  }

}
