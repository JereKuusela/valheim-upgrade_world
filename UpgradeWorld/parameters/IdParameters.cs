using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class IdParameters : FiltererParameters
{
  private List<string> Include = [];
  private readonly List<string> Ignore = [];
  public HashSet<string> VegIds() => VegetationOperation.GetIds(Include, Ignore);
  public HashSet<string> Ids() => [.. Include];
  public bool RequireId;
  public bool Validate;
  public IdParameters(FiltererParameters pars) : base(pars)
  {
  }
  public IdParameters(Terminal.ConsoleEventArgs args, bool requireId, bool validate) : base(args)
  {
    RequireId = requireId;
    Validate = validate;
    foreach (var par in Unhandled.ToList())
    {
      var split = par.Split('=');
      var name = split[0];
      if (name == "id")
        Include = [.. split[1].Split(',')];
      else if (name == "ignore")
        Ignore = [.. split[1].Split(',')];
      else continue;
      Unhandled.Remove(par);
    }
  }
  public override bool Valid(Terminal terminal)
  {
    Include = [.. Unhandled.SelectMany(kvp => Parse.Split(kvp))];
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    if (RequireId && Include.Count() == 0)
    {
      Helper.Print(terminal, "Error: Missing ids.");
      return false;
    }
    var invalidIds = Include.Where(id => !id.Contains("*") && ZNetScene.instance.GetPrefab(id) == null);
    if (Validate && invalidIds.Count() > 0)
      Helper.Print(terminal, $"Warning: Entity id {string.Join(", ", invalidIds)} not recognized.");
    return true;
  }
  public IdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
  }
}
