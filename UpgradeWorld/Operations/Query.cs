namespace UpgradeWorld {
  /// <summary>Prints amount of affected zones by the config.</summary>
  public class Query : ZoneOperation {
    public Query(Terminal context) : base(context, Zones.GetAllZones(), new AllZones()) {
      Operation = "Query";
    }

    public override bool Execute() {
      ZonesToUpgrade = Filterer.FilterZones(ZonesToUpgrade, NeedsOperation);
      Print(ZonesToUpgrade.Length + " to " + Operation + " (" + Filterer.Message + ")");
      return true;
    }

    protected override bool ExecuteZone(Vector2i zone) {
      return true;
    }

    protected override bool NeedsOperation(Vector2i zone) {
      return true;
    }
  }
}