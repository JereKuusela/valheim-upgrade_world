using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationIdParameters : FiltererParameters
{
  public HashSet<string> Ids = [];
  public bool Log = false;
  public LocationIdParameters(FiltererParameters pars) : base(pars)
  {
  }
  public LocationIdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
    foreach (var par in Unhandled.ToList())
    {
      if (par == "log")
        Log = true;
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override bool Valid(Terminal terminal)
  {
    Ids = [.. Unhandled.SelectMany(kvp => Parse.Split(kvp)).Distinct()];
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    var invalidIds = Ids.Where(id => ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIds.Count() > 0)
      Helper.Print(terminal, ServerExecution.User, $"Warning: Location id {string.Join(", ", invalidIds)} not recognized.");
    return true;
  }
}
