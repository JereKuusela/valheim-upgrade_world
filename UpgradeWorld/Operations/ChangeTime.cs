using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Safely changes time.</summary>
  public class ChangeTime : BaseOperation {
    public ChangeTime(Terminal context, double time) : base(context) {
      Change(time);
    }
    private void Change(double time) {
      var zNet = ZNet.instance;
      var previousTicks = zNet.GetTime().Ticks;
      var newTime = zNet.GetTimeSeconds() + time;
      zNet.SetNetTime(newTime);
      var delta = zNet.GetTime().Ticks - previousTicks;
      var zoneControl = Settings.ZoneControlId.GetStableHashCode();
      var spawnZonesUpdated = 0;
      var zoneControlsUpdated = 0;
      var updated = new Dictionary<int, int>();
      var dataNames = Settings.TimeBasedDataNames;
      var parameters = dataNames.Select(par => par.GetStableHashCode()).ToHashSet();
      foreach (var parameter in parameters) updated.Add(parameter, 0);
      foreach (var zdo in ZDOMan.instance.m_objectsByID.Values) {
        if (zdo.m_longs == null) continue;
        zdo.m_timeCreated += delta;
        var changed = false;
        if (zdo.GetPrefab() == zoneControl) {
          zoneControlsUpdated++;
          foreach (var key in zdo.m_longs.Keys.ToList()) {
            if (zdo.m_longs[key] == 0) continue;
            changed = true;
            spawnZonesUpdated++;
            zdo.m_longs[key] += delta;
          }
        }
        foreach (var parameter in parameters) {
          if (!zdo.m_longs.ContainsKey(parameter) || zdo.m_longs[parameter] == 0) continue;
          zdo.m_longs[parameter] += delta;
          updated[parameter]++;
          changed = true;
        }
        if (changed) zdo.IncreseDataRevision();
      }
      if (Settings.Verbose) {
        Print("Updated " + spawnZonesUpdated + " spawn zones in " + zoneControlsUpdated + " of zones");
        foreach (var par in dataNames) {
          var id = par.GetStableHashCode();
          Print("Updated " + updated[id] + " of " + par);
        }
      }
      Print("Skipping " + time.ToString("0") + "s , Day: " + EnvMan.instance.GetDay(newTime));
    }
  }
}