using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class DataParameters : IdParameters
{
  public Range<int>? Level;
  public string LocationIds = "";
  public bool Log = false;
  public new bool RequireId;
  public DataParameters(FiltererParameters pars) : base(pars)
  {
  }
  public DataParameters(Terminal.ConsoleEventArgs args, bool requireId) : base(args, requireId)
  {
    foreach (var kvp in Unhandled)
    {
      if (kvp.Key == "level")
        Level = Parse.IntRange(kvp.Value);
      if (kvp.Key == "log")
        Log = true;
      if (kvp.Key == "location")
        LocationIds = kvp.Value;
    }
    Unhandled.Remove("level");
    Unhandled.Remove("log");
    Unhandled.Remove("location");
  }
  public override IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos)
  {
    zdos = base.FilterZdos(zdos);
    if (Level != null)
    {
      var emptyOk = Level.Min <= 1;
      zdos = zdos.Where(zdo =>
      {
        if (zdo.m_ints == null) return emptyOk;
        if (!zdo.m_ints.TryGetValue(Hash.Level, out var value)) return emptyOk;
        return Level.Min <= value && value <= Level.Max;
      });
    }
    if (LocationIds != "")
    {
      var ids = Parse.Split(LocationIds).Select(s => s.GetStableHashCode()).ToHashSet();
      zdos = zdos.Where(zdo => ids.Contains(zdo.GetInt(Hash.Location)));
    }
    return zdos;
  }


  public static new Dictionary<string, Func<int, List<string>?>> GetAutoComplete()
  {
    var autoComplete = FiltererParameters.GetAutoComplete();
    autoComplete["level"] = (int index) => index == 0 ? CommandWrapper.Info("level=<color=yellow>amount</color> or level=<color=yellow>min-max</color> | Levels of the creature.") : null;
    autoComplete["log"] = (int index) => index == 0 ? CommandWrapper.Info("Out put to log file instead of console.") : null;
    autoComplete["location"] = (int index) => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
    return autoComplete;
  }
  public static new List<string> Parameters = GetAutoComplete().Keys.OrderBy(s => s).ToList();
}
