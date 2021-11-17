using System.Collections.Generic;

namespace UpgradeWorld {
  /// <summary>Destroys everything in a zone so that the world generator can regenerate it.</summary>
  public class Destroy : ZoneOperation {
    public Destroy(Terminal context, FiltererParameters args) : base(context) {
      Operation = "Destroy";
      ZonesPerUpdate = Settings.DestroysPerUpdate;
      args.TargetZones = TargetZones.Generated;
      Filterers = FiltererFactory.Create(args);
    }

    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      var sectorObjects = new List<ZDO>();
      ZDOMan.instance.FindObjects(zone, sectorObjects);
      foreach (var zdo in sectorObjects) {
        if (Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == zdo.m_uid) {
          continue;
        }
        var position = zdo.GetPosition();
        if (zoneSystem.GetZone(position) == zone) {
          ZDOMan.instance.RemoveFromSector(zdo, zone);
        }
      }
      var locations = zoneSystem.m_locationInstances;
      if (locations.TryGetValue(zone, out var location)) {
        location.m_placed = false;
        zoneSystem.m_locationInstances[zone] = location;
      }
      zoneSystem.m_generatedZones.Remove(zone);
      return true;
    }

    protected override void OnEnd() {
      var destroyed = ZonesToUpgrade.Length - Failed;
      var text = Operation + " completed.";
      if (Settings.Verbose) text += " " + destroyed + " zones destroyed.";
      if (Failed > 0) text += " " + Failed + " errors.";
      Print(text);
      Print("Run distribute or genloc command to re-distribute the location instances (if needed).");
    }
  }
}