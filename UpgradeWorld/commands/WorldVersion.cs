using System.Collections.Generic;

namespace UpgradeWorld;
public class WorldVersionCommand
{
  private static List<string> Versions = new() { "legacy", "hh", "mistlands" };
  public WorldVersionCommand()
  {
    CommandWrapper.Register("world_gen", (int index) =>
    {
      if (index == 0) return Versions;
      return null;
    });
    new Terminal.ConsoleCommand("world_gen", "[version] - Sets the world generation version.", (args) =>
    {
      FiltererParameters pars = new(args);

      var version = -1;
      if (pars.Unhandled.Contains("legacy")) version = 0;
      if (pars.Unhandled.Contains("hh")) version = 1;
      if (pars.Unhandled.Contains("mistlands")) version = 2;
      if (version == -1)
      {
        Helper.Print(args.Context, "Error: Missing or incorrect version parameter.");
        return;
      }
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new WorldVersion(args.Context, version, pars.Start));
    }, optionsFetcher: () => Versions);
  }
}
