using System;
using UnityEngine;

namespace UpgradeWorld;
///<summary>Registers locations without placing them to the world.</summary>
public class RegisterLocation : BaseOperation
{
  public RegisterLocation(Terminal context, string id, Vector3 position) : base(context)
  {
    Register(id, position);
  }
  private void Register(string id, Vector3 position)
  {
    var hash = id.GetStableHashCode();
    var zs = ZoneSystem.instance;
    if (!zs.m_locationsByHash.TryGetValue(hash, out var location))
      throw new InvalidOperationException($"Location {id} not found.");
    var zone = ZoneSystem.GetZone(position);
    var num = ZDOMan.instance.SectorToIndex(zone);
    var zdos = ZDOMan.instance.m_objectsBySector[num];
    if (zdos != null)
    {
      foreach (var zdo in zdos)
      {
        if (zdo.m_prefab != LocationProxyHash) continue;
        if (zdo.GetInt(LocationHash) != hash) continue;
        position = zdo.GetPosition();
        Print($"Using position {Helper.PrintVectorXZY(position)} of already existing location.");
        break;
      }
    }
    zs.m_locationInstances.Remove(zone);
    zs.RegisterLocation(location, position, zs.IsZoneGenerated(zone));
    AddPin(position);
    Print($"Location {id} registered to {Helper.PrintVectorXZY(position)}.");
    Print("To actually spawn the registered locations, reset the zone or spawn them manually.");
  }
}
