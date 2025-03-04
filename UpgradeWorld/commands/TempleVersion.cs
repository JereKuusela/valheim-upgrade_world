using System.Collections.Generic;

namespace UpgradeWorld;
public class TempleVersionCommand
{
  private static readonly List<string> Versions = ["mistlands", "ashlands"];
  public TempleVersionCommand()
  {
    CommandWrapper.Register("temple_gen", index =>
    {
      if (index == 0) return Versions;
      if (index == 1) return ["start"];
      return null;
    });
    Helper.Command("temple_gen", "[version] - Sets the altar generation version.", (args) =>
    {
      FiltererParameters pars = new(args);

      var version = "";
      if (pars.Unhandled.Contains("mistlands")) version = "mistlands";
      if (pars.Unhandled.Contains("ashlands")) version = "ashlands";
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new TempleVersion(args.Context, version, pars.Start));
    }, () => Versions);
  }
}
