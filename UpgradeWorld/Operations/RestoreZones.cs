using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
///<summary>Adds missing zone controls.</summary>
public class RestoreZones : ZoneOperation {
  protected int Added = 0;
  public RestoreZones(Terminal context, FiltererParameters args) : base(context, args) {
    Operation = "Restore zones";
    ZonesPerUpdate = Settings.DestroysPerUpdate;
    args.TargetZones = TargetZones.Generated;
    args.SafeZones = 0;
    InitString = args.Print($"Restore");
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone) {
    var zs = ZoneSystem.instance;
    var zonePos = zs.GetZonePos(zone);
    var prefab = zs.m_zoneCtrlPrefab;
    var hash = prefab.name.GetStableHashCode();
    List<ZDO> zdos = new();
    ZDOMan.instance.FindObjects(zone, zdos);
    var found = zdos.Any(zdo => zdo.GetPrefab() == hash);
    if (!found) {
      var zdo = ZDOMan.instance.CreateNewZDO(zonePos, hash);
      zdo.SetPrefab(hash);
      Added += 1;
    }
    return true;
  }
  protected override void OnEnd() {
    var text = Operation + " completed.";
    text += $" {Added} zone controls added.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}

