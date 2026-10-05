using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class VegetationIdParameters : FiltererParameters
{
  private readonly List<string> Include = [];
  private readonly List<string> Ignore = [];
  public HashSet<string> Ids = [];
  public VegetationIdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
    foreach (var par in Unhandled.ToList())
    {
      var split = par.Split('=');
      var name = split[0];
      if (name == "id")
        Include.AddRange(Parse.SplitWithEmpty(split[1]).Select(id => id.Trim()));
      else if (name == "ignore")
        Ignore.AddRange(Parse.SplitWithEmpty(split[1]).Select(id => id.Trim()));
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override bool Valid(Terminal terminal)
  {
    Include.AddRange(Unhandled.SelectMany(kvp => Parse.Split(kvp)));
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    var allIds = VegetationOperation.AllIds();
    var validIds = new HashSet<string>(allIds);
    var invalidIncludes = Include.Where(id => !id.Contains("*") && !validIds.Contains(id)).ToList();
    if (invalidIncludes.Count > 0)
    {
      Helper.Print(terminal, $"Error: Vegetation id {string.Join(", ", invalidIncludes)} not recognized.");
      return false;
    }
    var invalidIgnores = Ignore.Where(id => !id.Contains("*") && !validIds.Contains(id)).ToList();
    if (invalidIgnores.Count > 0)
    {
      Helper.Print(terminal, $"Error: Vegetation id {string.Join(", ", invalidIgnores)} not recognized.");
      return false;
    }
    Ids = [.. allIds.Where(id => (Include.Count == 0 || Include.Any(pattern => Helper.IsIncluded(pattern, id))) && !Ignore.Any(pattern => Helper.IsIncluded(pattern, id)))];
    if (Ids.Count == 0)
    {
      Helper.Print(terminal, "Error: No valid vegetation ids.");
      return false;
    }
    return true;
  }
}