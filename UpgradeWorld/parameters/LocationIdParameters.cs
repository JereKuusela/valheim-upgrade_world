using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class LocationIdParameters : FiltererParameters
{
  private List<string> Include = [];
  private readonly List<string> Ignore = [];
  public HashSet<string> Ids = [];
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
    var invalidIncludes = Include.Where(id => !id.Contains("*") && ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIncludes.Count() > 0)
    {
      Helper.Print(terminal, ServerExecution.User, $"Error: Location id {string.Join(", ", invalidIncludes)} not recognized.");
      return false;
    }
    var invalidIgnores = Ignore.Where(id => !id.Contains("*") && ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIgnores.Count() > 0)
    {
      Helper.Print(terminal, ServerExecution.User, $"Error: Location id {string.Join(", ", invalidIgnores)} not recognized.");
      return false;
    }
    Ids = LocationOperation.Ids(Include, Ignore);
    if (Ids.Count == 0)
    {
      Helper.Print(terminal, ServerExecution.User, "Error: No valid location ids.");
      return false;
    }
    return true;
  }
}
