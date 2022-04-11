namespace UpgradeWorld;
public class BiomesCountCommand {
  public BiomesCountCommand() {
    CommandWrapper.Register("biomes_count", (int index) => {
      if (index == 0) return CommandWrapper.Info("How precisely the biome is checked (meters). Lower value increases precision but takes longer to measure.");
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("biomes_count", "[precision] [...args] - Counts biomes by sampling points with a given precision (meters).", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters pars = new(args);
      var precision = 100f;
      if (pars.Unhandled.Count < 1) {
        args.Context.AddString("Error: Missing precision.");
        return;
      }
      if (!Parse.TryFloat(pars.Unhandled[0], out precision)) {
        args.Context.AddString("Error: Precision has wrong format.");
        return;
      }
      if (pars.Zone.HasValue) {
        args.Context.AddString("Error: <color=yellow>zone</color> is not supported.");
        return;
      }
      pars.Unhandled.RemoveAt(0);
      if (pars.Valid(args.Context))
        new CountBiomes(args.Context, precision, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
