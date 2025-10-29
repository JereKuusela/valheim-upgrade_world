using System.Linq;
namespace UpgradeWorld;
///<summary>Adds missing zone controls.</summary>
public class RestoreZones : ZoneOperation
{
  protected int Added = 0;
  public RestoreZones(Terminal context, FiltererParameters args) : base(context, args)
  {
    Operation = "Restore zones";
    args.TargetZones = TargetZones.Generated;
    args.SafeZones = 0;
    InitString = args.Print($"Restore");
    Filterers = FiltererFactory.Create(args);
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    var zonePos = ZoneSystem.GetZonePos(zone);
    var prefab = zs.m_zoneCtrlPrefab;
    var hash = prefab.name.GetStableHashCode();
    var zdos = Helper.GetZDOs(zone);
    var found = zdos != null && zdos.Any(zdo => zdo.m_prefab == hash);
    if (!found)
    {
      var zdo = ZDOMan.instance.CreateNewZDO(zonePos, hash);
      zdo.SetPrefab(hash);
      Added += 1;
    }
    return true;
  }
  protected override void OnEnd()
  {
    var text = Operation + " completed.";
    text += $" {Added} zone controls added.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}

