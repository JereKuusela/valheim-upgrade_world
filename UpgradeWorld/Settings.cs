

using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace UpgradeWorld {
  public struct FilterPoint {
    public float x;
    public float y;
    public float min;
    public float max;
  }
  public static class Settings {
    public static ConfigEntry<bool> configVerbose;
    public static bool Verbose => configVerbose.Value;
    public static ConfigEntry<bool> configAutoStart;
    public static bool AutoStart => configAutoStart.Value;
    public static ConfigEntry<float> configPlayerSafeDistance;
    public static float PlayerSafeDistance => configPlayerSafeDistance.Value;
    public static ConfigEntry<string> configCustomPoints;
    private static string CustomPoints => configCustomPoints.Value;
    public static ConfigEntry<string> configSafeZoneItems;
    public static HashSet<int> SafeZoneItems => configSafeZoneItems.Value.Split(',').Select(name => name.Trim().GetStableHashCode()).ToHashSet();
    public static ConfigEntry<int> configSafeZoneSize;
    public static int SafeZoneSize => configSafeZoneSize.Value;
    public static ConfigEntry<int> configDestroysPerUpdate;
    public static int DestroysPerUpdate => configDestroysPerUpdate.Value;
    public static ConfigEntry<string> configTimeBasedDataNames;
    public static IEnumerable<string> TimeBasedDataNames => configTimeBasedDataNames.Value.Split(',').Select(name => name.Trim());
    public static ConfigEntry<string> configZoneControlId;
    public static string ZoneControlId => configZoneControlId.Value.Trim();


    public static void Init(ConfigFile config) {
      var section = "1. General";
      configVerbose = config.Bind(section, "Verbose output", false, "If true, more detailed is printed (useful for debugging but may contain spoilers).");
      configAutoStart = config.Bind(section, "Automatic start", false, "If true, operations start automatically without having to use the start command.");
      configPlayerSafeDistance = config.Bind(section, "Safe distance around the player", 0f, "Zones within this distance won't be changed.");
      configCustomPoints = config.Bind(section, "Custom points", "", "List of coordinates and ranges to filter zones. Format: x1,z1,min1,max1,comment1|x2,z2,min2,max2,comment2|...");
      configSafeZoneItems = config.Bind(section, "Safe zone items", "blastfurnace,bonfire,charcoal_kiln,fermenter,fire_pit,forge,guard_stone,hearth,piece_artisanstation,piece_bed02,piece_brazierceiling01,piece_groundtorch,piece_groundtorch_blue,piece_groundtorch_green,piece_groundtorch_wood,piece_oven,piece_spinningwheel,piece_stonecutter,piece_walltorch,piece_workbench,portal,portal_wood,smelter,windmill", "List of entity names that prevent zones being modified.");
      configSafeZoneSize = config.Bind(section, "How many adjacent zones are included in the safe zone", 1, "-1 = disable, 0 = only the zone, 1 = 3x3 zones, 2 = 5x5 zones, etc.");

      configDestroysPerUpdate = config.Bind("2. Destroying", "Operations per update", 100, "How many zones are destroyed per Unity update.");
      configTimeBasedDataNames = config.Bind("3. Change time/day", "Time based data names", "spawntime,lastTime,SpawnTime,StartTime,alive_time,spawn_time,picked_time,plantTime,pregnant,TameLastFeeding", "Names of the data values that should be updated with the new time. Changing these is NOT recommended.");
      configZoneControlId = config.Bind("3. Change time/day", "Zone control name", "_ZoneCtrl", "Name of the zone control entity which controls the enemy spawning. Changing these is NOT recommended.");

    }
    /// <summary>Returns points and ranges to filter zones.</summary>
    public static FilterPoint[] GetFilterPoints() {
      var points = CustomPoints.Length > 0 ? CustomPoints.Split('|').Select(pointStr => {
        var parts = pointStr.Split(',');
        var point = new FilterPoint
        {
          x = float.Parse(parts[0]),
          y = float.Parse(parts[1]),
          min = float.Parse(parts[2]),
          max = float.Parse(parts[3])
        };
        return point;
      }).ToArray() : new FilterPoint[0];
      return points;
    }
  }
}