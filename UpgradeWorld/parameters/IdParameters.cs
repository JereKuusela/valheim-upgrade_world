using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class IdParameters : FiltererParameters
{
  public List<string> Ids = new();
  public bool RequireId;
  public bool Validate;
  public IdParameters(FiltererParameters pars) : base(pars)
  {
  }
  public IdParameters(Terminal.ConsoleEventArgs args, bool requireId, bool validate) : base(args)
  {
    RequireId = requireId;
    Validate = validate;
  }
  public override bool Valid(Terminal terminal)
  {
    Ids = Unhandled.SelectMany(kvp => Parse.Split(kvp)).ToList();
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    if (RequireId && Ids.Count() == 0)
    {
      Helper.Print(terminal, "Error: Missing ids.");
      return false;
    }
    var invalidIds = Ids.Where(id => !id.Contains("*") && ZNetScene.instance.GetPrefab(id) == null);
    if (Validate && invalidIds.Count() > 0)
      Helper.Print(terminal, $"Warning: Entity id {string.Join(", ", invalidIds)} not recognized.");
    return true;
  }
  public IdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
  }
}
