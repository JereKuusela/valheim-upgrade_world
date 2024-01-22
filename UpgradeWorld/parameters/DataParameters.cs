using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class DataParameters : IdParameters
{
  public Range<int>? Level;
  public string LocationIds = "";
  public List<string> Prints = [];
  public List<string> Datas = [];
  public List<string> Filters = [];
  public bool Log = false;
  public new bool RequireId;
  public List<string[]> Types = [];
  public DataParameters(FiltererParameters pars) : base(pars)
  {
  }
  public DataParameters(Terminal.ConsoleEventArgs args, bool requireId, bool validate = true) : base(args, requireId, validate)
  {
    foreach (var par in Unhandled.ToList())
    {
      var split = par.Split('=');
      var name = split[0];
      var value = string.Join("=", split.Skip(1));
      if (name == "level")
        Level = Parse.IntRange(value);
      else if (name == "log")
        Log = true;
      else if (name == "location")
        LocationIds = value;
      else if (name == "print")
        Prints.Add(value);
      else if (name == "data")
        Datas.Add(value);
      else if (name == "filter")
        Filters.Add(value);
      else if (name == "type") Types.Add(Parse.Split(split[1]));
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, bool checkExcludedZones)
  {
    zdos = base.FilterZdos(zdos, checkExcludedZones);
    if (Level != null)
    {
      var emptyOk = Level.Min <= 1;
      zdos = zdos.Where(zdo =>
      {
        if (!ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var ints)) return emptyOk;
        if (!ints.TryGetValue(ZDOVars.s_level, out var value)) return emptyOk;
        return Level.Min <= value && value <= Level.Max;
      });
    }
    if (LocationIds != "")
    {
      var ids = Parse.Split(LocationIds).Select(s => s.GetStableHashCode()).ToHashSet();
      zdos = zdos.Where(zdo => ids.Contains(zdo.GetInt(ZDOVars.s_location)));
    }
    foreach (var filter in Filters)
    {
      var split = Parse.Split(filter);
      var value = split.Length > 1 ? split[1] : "";
      var includeEmpty = split.Length > 2 && (Parse.Boolean(split[2]) ?? false);
      zdos = zdos.Where(zdo => DataHelper.HasData(zdo, split[0], value, includeEmpty));
    }
    return zdos;
  }


  public static new Dictionary<string, Func<int, List<string>?>> GetAutoComplete()
  {
    List<string> types = ["float", "id", "int", "long", "quat", "string", "vector"];
    List<string> truths = ["true", "false"];
    var autoComplete = FiltererParameters.GetAutoComplete();
    autoComplete["level"] = (int index) => index == 0 ? CommandWrapper.Info("level=<color=yellow>amount</color> or level=<color=yellow>min-max</color> | Levels of the creature.") : null;
    autoComplete["print"] = (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("print=<color=yellow>key</color>,type | Prints data with a given key.");
      if (index == 1) return types;
      return null;
    };
    autoComplete["data"] = (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("data=<color=yellow>key</color>,value,type | Sets data to a given key. Type is required for new entries.");
      if (index == 1) return CommandWrapper.Info("data=key,<color=yellow>value</color>,type | Data value.");
      if (index == 2) return types;
      return null;
    };

    autoComplete["filter"] = (int index) =>
    {
      if (index == 0) return CommandWrapper.Info("filter=<color=yellow>key</color>,value,includeEmpty | Filter by a data value.");
      if (index == 1) return CommandWrapper.Info("filter=key,<color=yellow>value</color>,includeEmpty | Value or data type for new entries.");
      if (index == 2) return truths;
      return null;
    };
    autoComplete["type"] = (int index) => ParameterComponents;
    autoComplete["log"] = (int index) => index == 0 ? CommandWrapper.Info("Out put to log file instead of console.") : null;
    autoComplete["location"] = (int index) => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
    return autoComplete;
  }
  public static new List<string> Parameters = [.. GetAutoComplete().Keys.OrderBy(s => s)];


  private static List<string> parameterComponents = [];
  public static List<string> ParameterComponents
  {
    get
    {
      if (parameterComponents.Count == 0)
        parameterComponents = ComponentInfo.Types.Select(t => t.Name).ToList();
      return parameterComponents;
    }
  }
}
