using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Safely sets the time.</summary>
public abstract class TimeOperation : BaseOperation
{
  public TimeOperation(Terminal context) : base(context) { }
  protected void Change(double time)
  {
    if (time < 0)
    {
      Print("Error: New time would be negative.");
      return;
    }
    var zNet = ZNet.instance;
    var previousTicks = zNet.GetTime().Ticks;
    zNet.SetNetTime(time);
    var delta = zNet.GetTime().Ticks - previousTicks;
    var spawnZonesUpdated = 0;
    var zoneControlsUpdated = 0;
    Dictionary<int, int> updated = [];
    var dataNames = Settings.TimeBasedDataNames;
    var parameters = dataNames.Select(par => par.GetStableHashCode()).ToHashSet();
    foreach (var parameter in parameters) updated.Add(parameter, 0);
    foreach (var zdo in ZDOMan.instance.m_objectsByID.Values)
    {
      if (!ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out var longs)) continue;
      var changed = false;
      if (zdo.GetPrefab() == Settings.ZoneControlHash)
      {
        zoneControlsUpdated++;
        foreach (var key in longs.Keys.ToList())
        {
          if (longs[key] == 0) continue;
          changed = true;
          spawnZonesUpdated++;
          longs[key] = (long)time;
        }
      }
      foreach (var parameter in parameters)
      {
        if (!longs.ContainsKey(parameter) || longs[parameter] == 0) continue;
        longs[parameter] = (long)time;
        updated[parameter]++;
        changed = true;
      }
      if (changed) zdo.IncreaseDataRevision();
    }
    if (Settings.Verbose)
    {
      Print("Updated " + spawnZonesUpdated + " spawn zones in " + zoneControlsUpdated + " of zones");
      foreach (var par in dataNames)
      {
        var id = par.GetStableHashCode();
        Print("Updated " + updated[id] + " of " + par);
      }
    }
    Print("Setting time to " + time.ToString("0") + "s , Day: " + EnvMan.instance.GetDay(time));
  }
}
