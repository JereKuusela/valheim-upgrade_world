using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld;
/// <summary>Removes missing objects from the world.</summary>
public class CleanDuplicates : EntityOperation
{
  public CleanDuplicates(Terminal context, Minimap.PinType? pin, bool alwaysPrint, FiltererParameters args) : base(context, pin)
  {
    Clean(alwaysPrint, args);
  }

  private void Clean(bool alwaysPrint, FiltererParameters args)
  {
    var generated = ZoneSystem.instance.m_generatedZones;
    HashSet<ZDO> toRemove = [];
    foreach (var group in GetZDOs(args).GroupBy(zdo => zdo.GetSector()))
    {
      if (!generated.Contains(group.Key)) continue;
      var sectorObjects = group.ToList();
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
