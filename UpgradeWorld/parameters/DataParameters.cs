using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class DataParameters : FiltererParameters {
  public List<string> Ids = new();
  public Range<int>? Level;
  public bool RequireId;
  public DataParameters(FiltererParameters pars) : base(pars) {
  }
  public DataParameters(Terminal.ConsoleEventArgs args, bool requireId) : base(args) {
    RequireId = requireId;
    List<string> unhandled = new();
    foreach (var par in Unhandled) {
      var split = par.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) {
        unhandled.Add(par);
        continue;
      }
      var value = split[1];
      if (name == "level")
        Level = Parse.TryIntRange(value);
      else
        unhandled.Add(par);
    }
    Ids = unhandled.SelectMany(Parse.Split).ToList();
    Unhandled.Clear();
  }
  public override bool Valid(Terminal terminal) {
    if (!base.Valid(terminal)) return false;
    if (RequireId && Ids.Count() == 0) {
      Helper.Print(terminal, "Error: Missing ids.");
      return false;
    }
    return true;
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
    autoComplete["level"] = (int index) => index == 0 ? CommandWrapper.Info("level=<color=yellow>amount</color> or level=<color=yellow>min-max</color> | Levels of the creature ().") : null;
    return autoComplete;
  }
  public static new List<string> Parameters =  GetAutoComplete().Keys.OrderBy(s => s).ToList();
}
