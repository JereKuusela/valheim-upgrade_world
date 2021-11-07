

using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace UpgradeWorld {
  public struct FilterPoint {
    public float x;
    public float y;
    public float min;
    public float max;
  }
  public static class Settings {
    public static ConfigEntry<float> configMinDistanceFromCenter;
    private static float MinDistanceFromCenter => configMinDistanceFromCenter.Value;
    public static ConfigEntry<bool> configDestroyLoadedAreas;
    public static bool DestroyLoadedAreas => configDestroyLoadedAreas.Value;
    public static ConfigEntry<bool> configVerbose;
    public static bool Verbose => configVerbose.Value;
    public static ConfigEntry<bool> configLocationsExcludePlayerBases;
    public static bool LocationsExcludePlayerBases => configLocationsExcludePlayerBases.Value;
    public static ConfigEntry<float> configMaxDistanceFromCenter;
    private static float MaxDistanceFromCenter => configMaxDistanceFromCenter.Value;
    public static ConfigEntry<float> configMinDistanceFromPlayer;
    private static float MinDistanceFromPlayer => configMinDistanceFromPlayer.Value;
    public static ConfigEntry<float> configMaxDistanceFromPlayer;
    private static float MaxDistanceFromPlayer => configMaxDistanceFromPlayer.Value;
    public static ConfigEntry<string> configCustomPoints;
    private static string CustomPoints => configCustomPoints.Value;
    public static ConfigEntry<int> configDestroysPerUpdate;
    public static int DestroysPerUpdate => configDestroysPerUpdate.Value;

    public static void Init(ConfigFile config) {
      var section = "General";
      configVerbose = config.Bind(section, "Verbose output", false, "If true, more detailed is printed (useful for debugging but may contain spoilers).");
      configMinDistanceFromCenter = config.Bind(section, "Minimum distance from the center", 0f, "Zones must be fully outside this distance to get upgraded.");
      configMaxDistanceFromCenter = config.Bind(section, "Maximum distance from the center", 0f, "Zones must be fully inside this distance to get upgraded. 0 for infinite.");
      configMinDistanceFromPlayer = config.Bind(section, "Minimum distance from the player", 0f, "Zones must be fully outside this distance to get upgraded.");
      configMaxDistanceFromPlayer = config.Bind(section, "Maximum distance from the player", 0f, "Zones must be fully inside this distance to get upgraded. 0 for infinite.");
      configCustomPoints = config.Bind(section, "Custom points", "", "List of coordinates and ranges to filter zones. Format: x1,z1,min1,max1,comment1|x2,z2,min2,max2,comment2|...");

      configLocationsExcludePlayerBases = config.Bind("Locations", "Exclude player bases", true, "If true, locations won't be placed inside player bases.");

      configDestroyLoadedAreas = config.Bind("Destroying", "Destroy loaded areas", false, "If true, loaded areas are also destroyed. USE AT YOUR WORN RISK!");
      configDestroysPerUpdate = config.Bind("Destroying", "Operations per update", 100, "How many zones are destroyed per Unity update.");
    }
    /// <summary>Returns points and ranges to filter zones.</summary>
    public static FilterPoint[] GetFilterPoints(Vector3 player) {
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
      points = points.Append(new FilterPoint
      {
        x = 0f,
        y = 0f,
        min = MinDistanceFromCenter,
        max = MaxDistanceFromCenter
      }).ToArray();
      if (player.magnitude > 0) {
        points = points.Append(new FilterPoint
        {
          x = player.x,
          y = player.y,
          min = MinDistanceFromPlayer,
          max = MaxDistanceFromPlayer
        }).ToArray();
      }
      return points;
    }

  }
}