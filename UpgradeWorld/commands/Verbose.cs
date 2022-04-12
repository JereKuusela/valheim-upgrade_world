namespace UpgradeWorld;
public class VerboseCommand {
  public VerboseCommand() {
    CommandWrapper.RegisterEmpty("verbose");
    new Terminal.ConsoleCommand("verbose", "- Toggles the verbose mode.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      Settings.configVerbose.Value = !Settings.Verbose;
      if (Settings.Verbose)
        Helper.Print(args.Context, "Verbose mode enabled.");
      else
        Helper.Print(args.Context, "Verbose mode disabled.");
    });
  }
}
