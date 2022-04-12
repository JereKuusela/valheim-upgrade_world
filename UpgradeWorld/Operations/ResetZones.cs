using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Destroys everything in a zone so that the world generator can regenerate it.</summary>
public class ResetZones : ZoneOperation {
  public ResetZones(Terminal context, FiltererParameters args) : base(context, args.Start) {
    Operation = "Reset";
    ZonesPerUpdate = Settings.DestroysPerUpdate;
    args.TargetZones = TargetZones.Generated;
    InitString = args.Print("Reset");
    Filterers = FiltererFactory.Create(args);
  }

  protected override bool ExecuteZone(Vector2i zone) {
    var zoneSystem = ZoneSystem.instance;
    var scene = ZNetScene.instance;
    List<ZDO> sectorObjects = new();
    ZDOMan.instance.FindObjects(zone, sectorObjects);
    var players = ZNet.instance.m_players.Select(player => player.m_characterID).ToHashSet();
    foreach (var zdo in sectorObjects) {
      if (players.Contains(zdo.m_uid)) continue;
      var position = zdo.GetPosition();
      if (zoneSystem.GetZone(position) == zone)
        Helper.RemoveZDO(zdo);
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
    if (Settings.Verbose) text += " " + destroyed + " zones reseted.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
    Print("Run genloc command to re-distribute the location instances (if needed).");
  }
}
