namespace UpgradeWorld;
/// <summary>Generates zones.</summary>
public class Generate : ZoneOperation {
  public Generate(Terminal context, FiltererParameters args) : base(context, args.ForceStart) {
    Operation = "Generate";
    InitString = args.Print("Generate");
    Filterers = FiltererFactory.Create(args);
  }

  protected override bool ExecuteZone(Vector2i zone) {
    var zoneSystem = ZoneSystem.instance;
    if (zoneSystem.IsZoneGenerated(zone)) return true;
    return zoneSystem.PokeLocalZone(zone);
  }

  protected override void OnEnd() {
    var generated = ZonesToUpgrade.Length - Failed;
    var text = Operation + " completed.";
    if (Settings.Verbose) text += " " + generated + " zones generated.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
  }
}
