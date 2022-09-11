using System.Linq;
using Service;

namespace UpgradeWorld;
public class BiomesCountCommand {
  public BiomesCountCommand() {
    CommandWrapper.Register("biomes_count", (int index) => {
      if (index == 0) return CommandWrapper.Info("How precisely the biome is checked (meters). Lower value increases precision but takes longer to measure.");
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("biomes_count", "[precision] [...args] - Counts biomes by sampling points with a given precision (meters).", (args) => {
      FiltererParameters pars = new(args);
      var precision = 100f;
      if (pars.Unhandled.Count < 1) {
        Helper.Print(args.Context, "Error: Missing precision.");
        return;
      }
      if (!Parse.TryFloat(pars.Unhandled.First().Key, out precision)) {
        Helper.Print(args.Context, "Error: Precision has wrong format.");
        return;
      }
      if (pars.Zone.HasValue) {
        Helper.Print(args.Context, "Error: <color=yellow>zone</color> is not supported.");
        return;
      }
      pars.Unhandled.Remove(pars.Unhandled.First().Key);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CountBiomes(args.Context, precision, pars);
    });
  }
}
