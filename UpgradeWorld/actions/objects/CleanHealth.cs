
using System.Collections.Generic;

namespace UpgradeWorld;
/// <summary>Removes excess health data from creatures and structures.</summary>
public class CleanHealth : EntityOperation
{
  public CleanHealth(Terminal context, ZDO[] zdos, bool pin) : base(context, pin)
  {
    Clean(zdos);
  }

  private void Clean(ZDO[] zdos)
  {
    var scene = ZNetScene.instance;
    FindPrefabs();
    var updated = 0;
    foreach (var zdo in zdos)
    {
      if (!Structures.Contains(zdo.m_prefab)) continue;
      var health = zdo.GetFloat(ZDOVars.s_health);
      if (health <= 0f) continue;
      var field = zdo.GetFloat(WearNTearField);
      if (field > 0 && health != field) continue;
      var component = scene.GetPrefab(zdo.m_prefab)?.GetComponent<WearNTear>();
      if (component?.m_health != health) continue;
      zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.RemoveFloat(ZDOVars.s_health);
      zdo.IncreaseDataRevision();
      updated++;
    }
    if (updated > 0)
      Print($"Cleared {updated} health value{S(updated)} from structure.");

    updated = 0;
    foreach (var zdo in zdos)
    {
      if (!Characters.Contains(zdo.m_prefab)) continue;
      var health = zdo.GetFloat(ZDOVars.s_health);
      if (health <= 0f) continue;
      var field = zdo.GetFloat(ZDOVars.s_maxHealth);
      if (health != field) continue;
      zdo.SetOwner(ZDOMan.GetSessionID());
      zdo.RemoveFloat(ZDOVars.s_health);
      zdo.IncreaseDataRevision();
      updated++;
    }
    if (updated > 0)
      Print($"Cleared {updated} health value{S(updated)} from creatures.");

  }

  private static readonly int WearNTearField = "WearNTear.m_health".GetStableHashCode();

  private static readonly HashSet<int> Structures = [];
  private static readonly HashSet<int> Characters = [];
  private void FindPrefabs()
  {
    if (Structures.Count > 0) return;
    var prefabs = ZNetScene.instance.m_namedPrefabs;
    foreach (var kvp in prefabs)
    {
      if (kvp.Value.GetComponent<WearNTear>())
        Structures.Add(kvp.Key);
      if (kvp.Value.GetComponent<Character>())
        Characters.Add(kvp.Key);
    }
  }
}
