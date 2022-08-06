using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class IdParameters : FiltererParameters {
  public List<string> Ids = new();
  public bool RequireId;
  public IdParameters(FiltererParameters pars) : base(pars) {
  }
  public IdParameters(Terminal.ConsoleEventArgs args, bool requireId) : base(args) {
    RequireId = requireId;
  }
  public override bool Valid(Terminal terminal) {
    Ids = Unhandled.SelectMany(kvp => Parse.Split(kvp.Key)).ToList();
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    if (RequireId && Ids.Count() == 0) {
      Helper.Print(terminal, "Error: Missing ids.");
      return false;
    }
    var invalidIds = Ids.Where(id => !id.Contains("*") && ZNetScene.instance.GetPrefab(id) == null);
    if (invalidIds.Count() > 0) {
      Print("Error: Invalid entity ids " + string.Join(", ", invalidIds));
      return false;
    }
    return true;
  }
  public IdParameters(Terminal.ConsoleEventArgs args) : base(args) {
  }
}
