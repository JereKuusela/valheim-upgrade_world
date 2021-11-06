using System;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {

  public abstract class ZoneOperation : BaseOperation {
    protected Vector2i[] ZonesToUpgrade;
    protected int ZonesPerUpdate = 1;
    protected int ZoneIndex = 0;
    protected ZoneFilterer Filterer;
    protected ZoneOperation(Terminal context, Vector2i[] zonesToUpgrade, ZoneFilterer filterer) : base(context) {
      ZonesToUpgrade = zonesToUpgrade;
      Filterer = filterer;
    }
    protected abstract bool NeedsOperation(Vector2i zone);
    protected abstract bool ExecuteZone(Vector2i zone);
    public override bool Execute() {
      if (ZonesToUpgrade == null || ZonesToUpgrade.Length == 0) return true;
      if (ZoneIndex == 0) {
        ZonesToUpgrade = Filterer.FilterZones(ZonesToUpgrade, NeedsOperation);
        Print(ZonesToUpgrade.Length + " to " + Operation + " (" + Filterer.Message + ")");
      }

      for (var i = 0; i < ZonesPerUpdate && ZoneIndex < ZonesToUpgrade.Length; i++) {
        var zone = ZonesToUpgrade[ZoneIndex];
        var success = ExecuteZone(zone);
        MoveToNextZone(success);
      }
      UpdateConsole();
      if (ZoneIndex >= ZonesToUpgrade.Length) {
        OnEnd();
        return true;
      }
      return false;
    }

    private void MoveToNextZone(bool success = true) {
      if (success) {
        Attempts = 0;
        ZoneIndex++;
      } else {
        Attempts++;
        if (Attempts > 100) {
          Failed++;
          Attempts = 0;
          ZoneIndex++;
        }
      }
    }
    private void UpdateConsole() {
      var totalString = ZonesToUpgrade.Length.ToString();
      var updatedString = ZoneIndex.ToString().PadLeft(totalString.Length, '0');
      var text = Operation + ": " + updatedString + "/" + totalString;
      Print(text);
    }
  }

  public interface ZoneFilterer {
    Vector2i[] FilterZones(Vector2i[] zones, Func<Vector2i, bool> needsOperation);
    string Message { get; set; }

  }
  public class AllZones : ZoneFilterer {
    public string Message { get; set; }

    public Vector2i[] FilterZones(Vector2i[] zones, Func<Vector2i, bool> needsOperation) {

      var generatedZones = zones.Length;
      zones = Filter.FilterByBiomes(zones, Settings.IncludedBiomes, Settings.ExcludedBiomes);
      var filteredByBiome = generatedZones - zones.Length;
      var filterPoints = Settings.GetFilterPoints(GetPlayerPosition());
      foreach (var filterPoint in filterPoints) {
        zones = Filter.FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
      }
      var filteredByPoints = generatedZones - filteredByBiome - zones.Length;
      zones = zones.Where(needsOperation).ToArray();
      var filteredByNeed = generatedZones - filteredByBiome - filteredByPoints - zones.Length;
      Message = "from " + generatedZones + " generated zones " + filteredByBiome + " filtered by biome, " + filteredByPoints + " filtered by position and " + filteredByNeed + " skipped";
      return zones;

    }
    private static Vector3 GetPlayerPosition() {
      var player = Player.m_localPlayer;
      return player ? player.transform.position : new Vector3(0, 0, 0);
    }

  }
  public class AdjacentZones : ZoneFilterer {
    private Vector2i Center;
    private int Adjacent;
    public AdjacentZones(Vector2i center, int adjacent) {
      Center = center;
      Adjacent = adjacent;
    }
    public string Message { get; set; }

    public Vector2i[] FilterZones(Vector2i[] zones, Func<Vector2i, bool> needsOperation) {
      zones = Filter.FilterByAdjacent(zones, Center, Adjacent);
      var includedZones = zones.Length;
      zones = zones.Where(needsOperation).ToArray();
      var filteredByNeed = includedZones - zones.Length;
      Message = filteredByNeed + " of " + includedZones + " were skipped";
      return zones;
    }
  }
  public class IncludedZones : ZoneFilterer {
    private Vector3 Center;
    private float Radius;
    public IncludedZones(Vector3 center, float radius) {
      Center = center;
      Radius = radius;
    }
    public string Message { get; set; }

    public Vector2i[] FilterZones(Vector2i[] zones, Func<Vector2i, bool> needsOperation) {
      zones = Filter.FilterByRadius(zones, Center, Radius);
      var includedZones = zones.Length;
      zones = zones.Where(needsOperation).ToArray();
      var filteredByNeed = includedZones - zones.Length;
      Message = filteredByNeed + " of " + includedZones + " were skipped";
      return zones;
    }
  }
}
