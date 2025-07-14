using System;
using UnityEngine;

namespace UpgradeWorld;
///<summary>Registers locations without placing them to the world.</summary>
public class UnregisterLocation : BaseOperation
{
  public UnregisterLocation(Terminal context, Vector3 position) : base(context)
  {
    Register(position);
  }
  private void Register(Vector3 position)
  {
    var zs = ZoneSystem.instance;
    var zone = ZoneSystem.GetZone(position);
    if (zs.m_locationInstances.TryGetValue(zone, out var instance))
      Print($"Location ${instance.m_location.m_prefabName} removed from {Helper.PrintVectorXZY(instance.m_position)}.");
    else
      Print($"No location registered in zone {zone}.");
  }
}
