using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveVegetation : ZoneOperation {
  protected int Removed = 0;
  private HashSet<int> Ids = new();
  public RemoveVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args.ForceStart) {
    Operation = "Remove vegetation";
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
    Ids = ids.Select(id => id.GetStableHashCode()).ToHashSet();
  }
  protected override bool ExecuteZone(Vector2i zone) {
    Remove(zone);
    return true;
  }
  protected void Remove(Vector2i zone) {
    var zs = ZoneSystem.instance;
    List<ZDO> zdos = new();
    ZDOMan.instance.FindObjects(zone, zdos);
    foreach (var zdo in zdos) {
      if (!Ids.Contains(zdo.GetPrefab())) return;
      Helper.RemoveZDO(zdo);
      Removed++;
    }
  }
  protected override void OnEnd() {
    base.OnEnd();
    var text = Operation + " completed.";
    if (Settings.Verbose) text += $" {Removed} vegetations removed.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
