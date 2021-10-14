namespace UpgradeWorld {

  public partial class Operations {
    /// <summary>Generates a zone.</summary>
    public static bool Generate(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return Patch.ZoneSystem_PokeLocalZone(zoneSystem, zone);
    }

    public static bool NeedsGenerating(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return !Patch.ZoneSystem_IsZoneGenerated(zoneSystem, zone);
    }
  }
}