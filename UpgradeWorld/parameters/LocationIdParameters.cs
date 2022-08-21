using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationIdParameters : FiltererParameters {
  public List<string> Ids = new();
  public bool RequireId;
  public LocationIdParameters(FiltererParameters pars) : base(pars) {
  }
  public LocationIdParameters(Terminal.ConsoleEventArgs args, bool requireId) : base(args) {
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
    var invalidIds = Ids.Where(id => !id.Contains("*") && ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIds.Count() > 0) {
      Helper.Print(terminal, "Error: Invalid locations ids " + string.Join(", ", invalidIds));
      return false;
    }
    return true;
  }
  public LocationIdParameters(Terminal.ConsoleEventArgs args) : base(args) {
  }
}
