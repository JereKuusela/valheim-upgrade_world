using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
public class IdParameters : FiltererParameters {
  public List<string> Ids = new();
  public IdParameters(Terminal.ConsoleEventArgs args) : base(args) {
    Ids = Unhandled.SelectMany(Parse.Split).ToList();
    Unhandled.Clear();
  }
}
