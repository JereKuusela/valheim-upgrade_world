using System.Linq;
using Service;

namespace UpgradeWorld;
public class LocationIdParameters : FiltererParameters
{
  public string[] Ids = new string[0];
  public LocationIdParameters(FiltererParameters pars) : base(pars)
  {
  }
  public LocationIdParameters(Terminal.ConsoleEventArgs args) : base(args)
  {
  }
  public override bool Valid(Terminal terminal)
  {
    Ids = Unhandled.SelectMany(kvp => Parse.Split(kvp.Key)).ToArray();
    Unhandled.Clear();
    if (!base.Valid(terminal)) return false;
    var invalidIds = Ids.Where(id => ZoneSystem.instance.GetLocation(id) == null);
    if (invalidIds.Count() > 0)
    {
      Helper.Print(terminal, ServerExecution.User, "Error: Invalid locations ids " + string.Join(", ", invalidIds));
      return false;
    }
    return true;
  }
}
