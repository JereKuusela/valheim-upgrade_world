using System;
using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Registers locations without placing them to the world.</summary>
public class RegisterLocations : ExecutedOperation
{
  ZoneSystem.ZoneLocation Location;
  FiltererParameters Args;
  public RegisterLocations(Terminal context, string id, FiltererParameters args) : base(context, args.Start)
  {
    Args = new(args);
    Args.TargetZones = TargetZones.All;

    var zs = ZoneSystem.instance;
    if (!zs.m_locationsByHash.TryGetValue(id.GetStableHashCode(), out Location))
      throw new InvalidOperationException($"Location {id} not found.");
  }
  protected override bool OnExecute()
  {
    var zs = ZoneSystem.instance;
    var filterers = FiltererFactory.Create(Args);
    List<string> messages = new();
    var zones = filterers.Aggregate(Zones.GetZones(Args), (zones, filterer) => filterer.FilterZones(zones, ref messages));
    var registered = 0;
    foreach (var zone in zones)
    {
      if (!Args.Roll()) continue;
      var randomPointInZone = zs.GetRandomPointInZone(zone, Location.m_exteriorRadius);
      zs.m_locationInstances.Remove(zone);
      zs.RegisterLocation(Location, randomPointInZone, zs.IsZoneGenerated(zone));
      registered++;
      if (Settings.Verbose)
        Print("Location " + Location.m_prefabName + " registered to " + zone.ToString());
    }
    Print($"Registered {registered} locations.");
    Print("To actually spawn the registered locations, reset the zone or spawn them manually.");
    return true;
  }

  protected override string OnInit()
  {
    return Args.Print($"Register location {Location.m_prefabName} to");
  }
}
