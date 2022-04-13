using System.Collections.Generic;
namespace UpgradeWorld;
/// <summary>Generates zones.</summary>
public class Generate : ZoneOperation {
  private List<ZoneSystem.ZoneVegetation> OriginalVegetation = new();
  public Generate(Terminal context, FiltererParameters args) : base(context, args.Start) {
    Operation = "Generate";
    InitString = args.Print("Generate");
    Filterers = FiltererFactory.Create(args);
  }

  protected override bool ExecuteZone(Vector2i zone) {
    var zoneSystem = ZoneSystem.instance;
    if (zoneSystem.IsZoneGenerated(zone)) return true;
    return zoneSystem.PokeLocalZone(zone);
  }
  protected override void OnStart() {
    OriginalVegetation = ZoneSystem.instance.m_vegetation;
    if (VegetationData.Load())
      Helper.Print(Context, User, $"{ZoneSystem.instance.m_vegetation.Count} vegetations loaded from vegetation.json");
  }
  protected override void OnEnd() {
    ZoneSystem.instance.m_vegetation = OriginalVegetation;
    var generated = ZonesToUpgrade.Length - Failed;
    var text = Operation + " completed.";
    if (Settings.Verbose) text += " " + generated + " zones generated.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
