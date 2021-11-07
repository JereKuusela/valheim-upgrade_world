using System;
using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {

  public abstract class ZoneOperation : BaseOperation {
    protected Vector2i[] ZonesToUpgrade;
    protected int ZonesPerUpdate = 1;
    protected int ZoneIndex = -1;
    protected ZoneFilterer[] Filterers;
    protected ZoneOperation(Terminal context, Vector2i[] zonesToUpgrade, ZoneFilterer[] filterers) : base(context) {
      ZonesToUpgrade = zonesToUpgrade;
      Filterers = filterers;
    }
    protected abstract bool ExecuteZone(Vector2i zone);
    protected void Init() {
      var amount = ZonesToUpgrade.Length;
      var messages = new List<string>();
      ZonesToUpgrade = Filterers.Aggregate(ZonesToUpgrade, (zones, filterer) => filterer.FilterZones(zones, ref messages));
      Print(Operation + ": " + ZonesToUpgrade.Length + " of " + amount + " zones (" + string.Join(", ", messages) + ")");
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
    // Track the same message to reduce clutter.
    private int PreviousPercent = -1;
    private int PreviousIndex = -1;
    private void UpdateConsole() {
      if (Settings.Verbose) {
        var totalString = ZonesToUpgrade.Length.ToString();
        var updatedString = ZoneIndex.ToString().PadLeft(totalString.Length, '0');
        if (ZoneIndex != PreviousIndex)
          Print(Operation + ": " + updatedString + "/" + totalString);
        PreviousIndex = ZoneIndex;
      } else {
        var percent = Math.Min(100, ZonesToUpgrade.Length == 0 ? 100 : (int)Math.Floor(100.0 * ZoneIndex / ZonesToUpgrade.Length));
        if (percent != PreviousPercent)
          Print(Operation + ": " + percent + "%");
        PreviousPercent = percent;
      }
    }
  }
}
