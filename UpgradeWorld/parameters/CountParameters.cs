using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class CountParameters : DataParameters {
  public Range<int> Count = new(0, int.MaxValue);
  public CountParameters(FiltererParameters pars) : base(pars) {
  }
  public CountParameters(Terminal.ConsoleEventArgs args) : base(args, false) {
    foreach (var kvp in Unhandled) {
      if (kvp.Key == "count")
        Count = Parse.IntRange(kvp.Value);
    }
    Unhandled.Remove("count");
  }

  public static new Dictionary<string, Func<int, List<string>?>> GetAutoComplete() {
    var autoComplete = DataParameters.GetAutoComplete();
    autoComplete["count"] = (int index) => index == 0 ? CommandWrapper.Info("count=<color=yellow>amount</color> or count=<color=yellow>min-max</color> | Required amount of objects to be included in the count.") : null;
    return autoComplete;
  }
  public static new List<string> Parameters = GetAutoComplete().Keys.OrderBy(s => s).ToList();
}
