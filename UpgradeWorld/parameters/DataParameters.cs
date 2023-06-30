using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class DataParameters : IdParameters {
  public Range<int>? Level;
  public string LocationIds = "";
  public List<string> Prints = new();
  public List<string> Datas = new();
  public List<string> Filters = new();
  public bool Log = false;
  public new bool RequireId;
  public DataParameters(FiltererParameters pars) : base(pars) {
  }
  public DataParameters(Terminal.ConsoleEventArgs args, bool requireId, bool validate = true) : base(args, requireId, validate) {
    foreach (var par in Unhandled.ToList()) {
      var split = par.Split('=');
      var value = string.Join("=", split.Skip(1));
      if (split[0] == "level")
        Level = Parse.IntRange(value);
      else if (split[0] == "log")
        Log = true;
      else if (split[0] == "location")
        LocationIds = value;
      else if (split[0] == "print")
        Prints.Add(value);
      else if (split[0] == "data")
        Datas.Add(value);
      else if (split[0] == "filter")
        Filters.Add(value);
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos) {
    zdos = base.FilterZdos(zdos);
    if (Level != null) {
      var emptyOk = Level.Min <= 1;
      zdos = zdos.Where(zdo => {
        if (!ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var ints)) return emptyOk;
        if (!ints.TryGetValue(ZDOVars.s_level, out var value)) return emptyOk;
        return Level.Min <= value && value <= Level.Max;
      });
    }
    if (LocationIds != "") {
      var ids = Parse.Split(LocationIds).Select(s => s.GetStableHashCode()).ToHashSet();
      zdos = zdos.Where(zdo => ids.Contains(zdo.GetInt(ZDOVars.s_location)));
    }
    foreach (var filter in Filters) {
      var split = Parse.Split(filter);
      var value = split.Length > 1 ? split[1] : "";
      var includeEmpty = split.Length > 2 && (Parse.Boolean(split[2]) ?? false);
      zdos = zdos.Where(zdo => DataHelper.HasData(zdo, split[0], value, includeEmpty));
    }
    return zdos;
  }


  public static new Dictionary<string, Func<int, List<string>?>> GetAutoComplete() {
    var types = new List<string>() { "float", "id", "int", "long", "quat", "string", "vector" };
    var truths = new List<string>() { "true", "false" };
    var autoComplete = FiltererParameters.GetAutoComplete();
    autoComplete["level"] = (int index) => index == 0 ? CommandWrapper.Info("level=<color=yellow>amount</color> or level=<color=yellow>min-max</color> | Levels of the creature.") : null;
    autoComplete["print"] = (int index) => {
      if (index == 0) return CommandWrapper.Info("print=<color=yellow>key</color>,type | Prints data with a given key.");
      if (index == 1) return types;
      return null;
    };
    autoComplete["data"] = (int index) => {
      if (index == 0) return CommandWrapper.Info("data=<color=yellow>key</color>,value,type | Sets data to a given key. Type is required for new entries.");
      if (index == 1) return CommandWrapper.Info("data=key,<color=yellow>value</color>,type | Data value.");
      if (index == 2) return types;
      return null;
    };

    autoComplete["filter"] = (int index) => {
      if (index == 0) return CommandWrapper.Info("filter=<color=yellow>key</color>,value,includeEmpty | Filter by a data value.");
      if (index == 1) return CommandWrapper.Info("filter=key,<color=yellow>value</color>,includeEmpty | Value or data type for new entries.");
      if (index == 2) return truths;
      return null;
    };
    autoComplete["log"] = (int index) => index == 0 ? CommandWrapper.Info("Out put to log file instead of console.") : null;
    autoComplete["location"] = (int index) => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
    return autoComplete;
  }
  public static new List<string> Parameters = GetAutoComplete().Keys.OrderBy(s => s).ToList();
}
