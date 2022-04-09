using System.Linq;
namespace UpgradeWorld;
public class CountBiomesCommand {
  public CountBiomesCommand() {
    new Terminal.ConsoleCommand("count_biomes", "[frequency] [...args] - Counts amounts of biomes with given meters of frequency.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters pars = new(args);
      var frequency = 100f;
      if (pars.Unhandled.Count < 1) {
        args.Context.AddString("Error: Missing frequency.");
        return;
      }
      if (!Parse.TryFloat(pars.Unhandled[0], out frequency)) {
        args.Context.AddString("Error: Frequency has wrong format.");
        return;
      }
      if (pars.Zone.HasValue) {
        args.Context.AddString("Error: <color=yellow>zone</color> is not supported.");
        return;
      }
      pars.Unhandled.RemoveAt(0);
      if (pars.Valid(args.Context))
        new CountBiomes(args.Context, frequency, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
