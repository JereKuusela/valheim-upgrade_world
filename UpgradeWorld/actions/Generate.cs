using UnityEngine;

namespace UpgradeWorld;
/// <summary>Generates zones.</summary>
public class Generate : ZoneOperation
{
  private bool Empty = false;
  public Generate(Terminal context, FiltererParameters args, bool empty) : base(context, args)
  {
    Operation = "Generate";
    InitString = args.Print("Generate");
    Filterers = FiltererFactory.Create(args);
    if (empty) ZonesPerUpdate = 100000;
    Empty = empty;
  }

  protected override bool ExecuteZone(Vector2i zone)
  {
    var zs = ZoneSystem.instance;
    if (Empty)
    {
      zs.m_generatedZones.Add(zone);
      return true;
    }
    if (zs.IsZoneGenerated(zone)) return true;
    return zs.SpawnZone(zone, ZoneSystem.SpawnMode.Ghost, out GameObject _);
  }
  protected override void OnEnd()
  {
    var generated = ZonesToUpgrade.Length - Failed;
    var text = Operation + " completed. " + generated + " zones generated.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
