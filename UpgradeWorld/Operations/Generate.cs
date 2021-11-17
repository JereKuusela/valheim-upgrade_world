namespace UpgradeWorld {
  /// <summary>Generates zones.</summary>
  public class Generate : ZoneOperation {
    public Generate(Terminal context, FiltererParameters args) : base(context) {
      Operation = "Generate";
      args.TargetZones = TargetZones.Ungenerated;
      args.NoPlayerBase = true;
      Filterers = FiltererFactory.Create(args);
    }

    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
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
}