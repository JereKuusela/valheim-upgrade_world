using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class DataParameters : IdParameters {
  public Range<int>? Level;
  public new bool RequireId;
  public DataParameters(FiltererParameters pars) : base(pars) {
  }
  public DataParameters(Terminal.ConsoleEventArgs args, bool requireId) : base(args, requireId) {
    foreach (var kvp in Unhandled) {
      if (kvp.Key == "level")
        Level = Parse.IntRange(kvp.Value);
    }
    Unhandled.Remove("level");
  }
  public override IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos) {
    zdos = base.FilterZdos(zdos);
    if (Level != null) {
      var emptyOk = Level.Min <= 1;
      zdos = zdos.Where(zdo => {
        if (zdo.m_ints == null) return emptyOk;
        if (!zdo.m_ints.TryGetValue(Hash.Level, out var value)) return emptyOk;
        return Level.Min <= value && value <= Level.Max;
      });
    }
    return zdos;
  }


  public static new Dictionary<string, Func<int, List<string>?>> GetAutoComplete() {
    var autoComplete = FiltererParameters.GetAutoComplete();
    autoComplete["level"] = (int index) => index == 0 ? CommandWrapper.Info("level=<color=yellow>amount</color> or level=<color=yellow>min-max</color> | Levels of the creature.") : null;
    return autoComplete;
  }
  public static new List<string> Parameters = GetAutoComplete().Keys.OrderBy(s => s).ToList();
}
