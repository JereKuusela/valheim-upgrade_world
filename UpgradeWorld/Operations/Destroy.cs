using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {
  /// <summary>Destroys everything in a zone so that the world generator can regenerate it.</summary>
  public abstract class Destroy : ZoneOperation {
    public Destroy(Terminal context, ZoneFilterer filterer) : base(context, Zones.GetAllZones(), filterer) {
      Operation = "Destroy";
      ZonesPerUpdate = Settings.DestroysPerUpdate;
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

    protected override bool NeedsOperation(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return Settings.DestroyLoadedAreas || !zoneSystem.IsZoneLoaded(zone);
    }

    protected override void OnEnd() {
      Print("Zones destroyed. Run distribute or genloc command to re-distribute the location instances.");
    }
  }

  public class DestroyAll : Destroy {
    public DestroyAll(Terminal context) : base(context, new AllZones()) {
    }
  }
  public class DestroyAdjacent : Destroy {
    public DestroyAdjacent(Vector2i center, int adjacent, Terminal context) : base(context, new AdjacentZones(center, adjacent)) {
    }
  }
  public class DestroyIncluded : Destroy {
    public DestroyIncluded(Vector3 center, float radius, Terminal context) : base(context, new IncludedZones(center, radius)) {
    }
  }
}