using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public class IdParameters : FiltererParameters {
  public List<string> Ids = new();
  public IdParameters(Terminal.ConsoleEventArgs args) : base(args) {
    Ids = Unhandled.SelectMany(Parse.Split).ToList();
    Unhandled.Clear();
  }
  public override bool Valid(Terminal terminal) {
    if (!base.Valid(terminal)) return false;
    if (Ids.Count() == 0) {
      Helper.Print(terminal, "Error: Missing location ids.");
      return false;
    }
    return true;
  }
}
