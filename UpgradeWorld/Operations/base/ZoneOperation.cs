using System;
using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  ///<summary>Base class for all zone based operations. Provides the "zone by zone" execution logic.</summary>

  public abstract class ZoneOperation : ExecutedOperation {
    public string Operation = "BaseOperation";
    protected Vector2i[] ZonesToUpgrade;
    protected int ZonesPerUpdate = 1;
    protected int ZoneIndex = 0;
    protected IEnumerable<ZoneFilterer> Filterers;
    protected ZoneOperation(Terminal context, bool autoStart) : base(context, autoStart) {
      ZonesToUpgrade = Zones.GetWorldZones();
    }
    protected string InitString = "";
    protected override string OnInit() {
      var messages = new List<string>();
      ZonesToUpgrade = Filterers.Aggregate(ZonesToUpgrade, (zones, filterer) => filterer.FilterZones(zones, ref messages));
      var zoneString = ZonesToUpgrade.Length + " zones (" + Helper.JoinRows(messages) + ")";
      if (Settings.Verbose)
        InitString += ": " + zoneString;
      return InitString;
    }
    protected abstract bool ExecuteZone(Vector2i zone);
    protected override bool OnExecute() {
      if (ZonesToUpgrade == null || ZonesToUpgrade.Length == 0) return true;

      for (var i = 0; i < ZonesPerUpdate && ZoneIndex < ZonesToUpgrade.Length; i++) {
        var zone = ZonesToUpgrade[ZoneIndex];
        var success = ExecuteZone(zone);
        MoveToNextZone(success);
      }
      UpdateConsole();
      if (ZoneIndex >= ZonesToUpgrade.Length) {
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
        if (Attempts > 1000) {
          Failed++;
          Attempts = 0;
          ZoneIndex++;
        }
      }
    }
    private void UpdateConsole() {
      if (Settings.Verbose) {
        var totalString = ZonesToUpgrade.Length.ToString();
        var updatedString = ZoneIndex.ToString().PadLeft(totalString.Length, '0');
        PrintOnce(Operation + ": " + updatedString + "/" + totalString, false);
      } else {
        var percent = Math.Min(100, ZonesToUpgrade.Length == 0 ? 100 : (int)Math.Floor(100.0 * ZoneIndex / ZonesToUpgrade.Length));
        PrintOnce(Operation + ": " + percent + "%", false);
      }
    }
  }
}
