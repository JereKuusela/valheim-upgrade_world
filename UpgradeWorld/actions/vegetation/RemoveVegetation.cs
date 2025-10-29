using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld;
/// <summary>Removes given entity ids within a given distance.</summary>
public class RemoveVegetation : ZoneOperation
{
  protected int Removed = 0;
  private readonly HashSet<int> Hashes = [];
  public RemoveVegetation(Terminal context, HashSet<string> ids, FiltererParameters args) : base(context, args)
  {
    Operation = "Remove vegetation";
    InitString = args.Print($"Remove vegetation{Helper.IdString(ids)} from");
    args.TargetZones = TargetZones.Generated;
    Filterers = FiltererFactory.Create(args);
    // No parameter -> all vegetation.
    if (ids.Count == 0)
      ids = [.. ZoneSystem.instance.m_vegetation.Select(veg => veg.m_prefab.name)];
    Hashes = [.. ids.Select(id => id.GetStableHashCode())];
    // Automatically clean up fractions as well.
    foreach (var id in ids)
    {
      if (ZNetScene.instance.GetPrefab(id + "_frac"))
        Hashes.Add((id + "_frac").GetStableHashCode());
    }
  }
  protected override bool ExecuteZone(Vector2i zone)
  {
    Remove(zone);
    return true;
  }
  protected void Remove(Vector2i zone)
  {
    var zdos = Helper.GetZDOs(zone);
    if (zdos == null) return;
    foreach (var zdo in zdos)
    {
      if (!Args.RollAmount()) continue;
      if (!Hashes.Contains(zdo.m_prefab)) continue;
      AddPin(zdo.GetPosition());
      Helper.RemoveZDO(zdo);
      Removed++;
    }
  }
  protected override void OnEnd()
  {
    base.OnEnd();
    var text = $"{Operation} completed. {Removed} vegetations removed.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
