using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationIdParameters : FiltererParameters
{
  private List<string> Include = [];
  private readonly List<string> Ignore = [];
  public HashSet<string> Ids() => LocationOperation.Ids(Include, Ignore);
  public bool Log = false;
  public LocationIdParameters(FiltererParameters pars) : base(pars)
  {
  }
  public LocationIdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
    foreach (var par in Unhandled.ToList())
    {
      var split = par.Split('=');
      var name = split[0];
      if (name == "id")
        Include = [.. split[1].Split(',')];
      else if (name == "ignore")
        Ignore = [.. split[1].Split(',')];
      else if (name == "log")
        Log = true;
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override bool Valid(Terminal terminal)
  {
    Include = [.. Unhandled.SelectMany(kvp => Parse.Split(kvp)).Distinct()];
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    var invalidIds = Include.Where(id => ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIds.Count() > 0)
      Helper.Print(terminal, ServerExecution.User, $"Warning: Location id {string.Join(", ", invalidIds)} not recognized.");
    return true;
  }
}
