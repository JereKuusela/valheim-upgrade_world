namespace UpgradeWorld;
public class VerboseCommand
{
  public VerboseCommand()
  {
    CommandWrapper.RegisterEmpty("verbose");
    Helper.Command("verbose", "- Toggles the verbose mode.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      Settings.configVerbose.Value = !Settings.Verbose;
      if (Settings.Verbose)
        Helper.Print(args.Context, "Verbose mode enabled.");
      else
        Helper.Print(args.Context, "Verbose mode disabled.");
    });
  }
}
