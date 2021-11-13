using System;
using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {

  public abstract class ZoneOperation : BaseOperation {
    protected Vector2i[] ZonesToUpgrade;
    protected int ZonesPerUpdate = 1;
    protected int ZoneIndex = -1;
    protected ZoneFilterer[] Filterers;
    protected TargetZones TargetZones = TargetZones.Generated;
    protected ZoneOperation(Terminal context, ZoneFilterer[] filterers, TargetZones targetZones = TargetZones.Generated) : base(context) {
      ZonesToUpgrade = Zones.GetWorldZones();
      Filterers = filterers;
      TargetZones = targetZones;
    }
    protected abstract bool ExecuteZone(Vector2i zone);
    protected void Init(bool forceVerbose = false) {
      var messages = new List<string>();
      ZonesToUpgrade = new TargetZonesFilterer(TargetZones).FilterZones(ZonesToUpgrade, ref messages);
      ZonesToUpgrade = Filterers.Aggregate(ZonesToUpgrade, (zones, filterer) => filterer.FilterZones(zones, ref messages));
      if (Settings.Verbose || forceVerbose)
        Print(Operation + ": " + ZonesToUpgrade.Length + " zones (" + Helper.JoinRows(messages) + ")");
    }
    protected override bool OnExecute() {
      if (ZonesToUpgrade == null || ZonesToUpgrade.Length == 0) return true;
      if (ZoneIndex == -1) {
        Init();
        ZoneIndex = 0;
      }

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
