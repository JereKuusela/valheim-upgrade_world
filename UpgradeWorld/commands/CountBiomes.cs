using System.Linq;
namespace UpgradeWorld;
public class CountBiomesCommand {
  public CountBiomesCommand() {
    new Terminal.ConsoleCommand("count_biomes", "[frequency] [...args] - Counts amounts of biomes with given meters of frequency.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      var frequency = 100f;
      if (args.Args.Count() < 2) {
        args.Context.AddString("Error: Missing frequency.");
        return;
      }
      if (!Parse.TryFloat(args.Args[1], out frequency)) {
        args.Context.AddString("Error: Frequency has wrong format.");
        return;
      }
      // Remove only one was ParseFiltererArgs will also remove one.
      var extra = Parse.Args(args.Args, 1);
      FiltererParameters parameters = new();
      extra = Parse.FiltererArgs(extra, parameters);
      if (Helper.CheckUnhandled(args, extra))
        new CountBiomes(args.Context, frequency, parameters);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
