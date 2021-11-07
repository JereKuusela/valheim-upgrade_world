namespace UpgradeWorld {
  /// <summary>Prints amount of affected zones by the config.</summary>
  public class Query : ZoneOperation {
    public Query(Terminal context) : base(context, Zones.GetAllZones(), new ZoneFilterer[] { new ConfigFilterer() }) {
      Operation = "Query";
    }

    protected override bool OnExecute() {
      Init();
      return true;
    }

    protected override bool ExecuteZone(Vector2i zone) {
      return true;
    }
  }
}