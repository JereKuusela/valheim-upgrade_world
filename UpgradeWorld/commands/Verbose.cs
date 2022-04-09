namespace UpgradeWorld;
public class VerboseCommand {
  public VerboseCommand() {
    new Terminal.ConsoleCommand("verbose", "[off] - Toggles the verbose mode.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      Settings.configVerbose.Value = !Settings.Verbose;
      if (Settings.Verbose)
        args.Context.AddString("Verbose mode enabled.");
      else
        args.Context.AddString("Verbose mode disabled.");
    });
  }
}
