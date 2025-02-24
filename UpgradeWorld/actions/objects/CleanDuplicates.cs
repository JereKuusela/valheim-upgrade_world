using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld;
/// <summary>Removes missing objects from the world.</summary>
public class CleanDuplicates : EntityOperation
{
  public CleanDuplicates(Terminal context, bool pin, bool alwaysPrint) : base(context, pin)
  {
    Clean(alwaysPrint);
  }

  private void Clean(bool alwaysPrint)
  {
    var zones = ZoneSystem.instance.m_generatedZones;

    HashSet<ZDO> toRemove = [];
    foreach (var zone in zones)
    {
      var sectorObjects = Helper.GetZDOs(zone);
      if (sectorObjects == null) continue;
      for (var i = 0; i < sectorObjects.Count; i++)
      {
        var zdo = sectorObjects[i];
        for (var j = i + 1; j < sectorObjects.Count; j++)
        {
          var other = sectorObjects[j];
          if (zdo.m_prefab == other.m_prefab && zdo.m_position == other.m_position && zdo.m_rotation == other.m_rotation)
            toRemove.Add(other);
        }
      }
    }
    HashSet<Vector3> pins = [];
    var scene = ZNetScene.instance;
    foreach (var zdo in toRemove)
    {
      if (!pins.Contains(zdo.m_position))
      {
        AddPin(zdo.m_position);
        pins.Add(zdo.m_position);
      }
      if (Settings.Verbose)
        Print($"Removed duplicate object {scene.GetPrefab(zdo.m_prefab)?.name} at {zdo.m_position}.");
      Helper.RemoveZDO(zdo);
    }
    if (alwaysPrint || toRemove.Count > 0)
      Print($"Removed {toRemove.Count} duplicated object{S(toRemove.Count)}.");
  }

}
