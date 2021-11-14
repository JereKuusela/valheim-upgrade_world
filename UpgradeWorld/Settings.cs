

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
    public static ConfigEntry<bool> configClearLocationAreas;
    public static bool ClearLocationAreas => configClearLocationAreas.Value;
    public static ConfigEntry<bool> configVerbose;
    public static bool Verbose => configVerbose.Value;
    public static ConfigEntry<float> configPlayerSafeDistance;
    public static float PlayerSafeDistance => configPlayerSafeDistance.Value;
    public static ConfigEntry<string> configCustomPoints;
    private static string CustomPoints => configCustomPoints.Value;
    public static ConfigEntry<int> configDestroysPerUpdate;
    public static int DestroysPerUpdate => configDestroysPerUpdate.Value;

    public static void Init(ConfigFile config) {
      var section = "General";
      configVerbose = config.Bind(section, "Verbose output", false, "If true, more detailed is printed (useful for debugging but may contain spoilers).");
      configPlayerSafeDistance = config.Bind(section, "Safe distance around the player", 0f, "Zones within this distance won't be changed.");
      configCustomPoints = config.Bind(section, "Custom points", "", "List of coordinates and ranges to filter zones. Format: x1,z1,min1,max1,comment1|x2,z2,min2,max2,comment2|...");

      configClearLocationAreas = config.Bind("Locations", "Clear location areas", true, "If true, objects under places locatins will be removed.");
      configDestroysPerUpdate = config.Bind("Destroying", "Operations per update", 100, "How many zones are destroyed per Unity update.");
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