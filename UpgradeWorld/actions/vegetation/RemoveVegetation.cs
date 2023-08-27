using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveVegetation : ZoneOperation {
  protected int Removed = 0;
  private readonly HashSet<int> Ids = new();
  public RemoveVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args) {
    ZonesPerUpdate = Settings.DestroysPerUpdate;
    Operation = "Remove vegetation";
    InitString = args.Print($"Remove vegetation{Helper.IdString(ids)} from");
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
    // No parameter -> all vegetation.
    if (ids.Count == 0)
      ids = ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name).ToHashSet();
    Ids = ids.Select(id => id.GetStableHashCode()).ToHashSet();
    // Automatically clean up fractions as well.
    foreach (var id in ids) {
      if (ZNetScene.instance.GetPrefab(id + "_frac"))
        Ids.Add((id + "_frac").GetStableHashCode());
    }
  }
  protected override bool ExecuteZone(Vector2i zone) {
    Remove(zone);
    return true;
  }
  protected void Remove(Vector2i zone) {
    List<ZDO> zdos = new();
    ZDOMan.instance.FindObjects(zone, zdos);
    foreach (var zdo in zdos) {
      if (!Args.Roll()) continue;
      if (!Ids.Contains(zdo.GetPrefab())) continue;
      Helper.RemoveZDO(zdo);
      Removed++;
    }
  }
  protected override void OnEnd() {
    var text = $"{Operation} completed. {Removed} vegetations removed.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
