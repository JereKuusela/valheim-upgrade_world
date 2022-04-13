namespace UpgradeWorld;
public class VegetationExportCommand {
  public VegetationExportCommand() {
    CommandWrapper.RegisterEmpty("vegetation_export");
    new Terminal.ConsoleCommand("vegetation_export", "- Saves current vegetation data to a file.", (Terminal.ConsoleEventArgs args) => {
      if (Helper.IsClient(args)) return;
      if (VegetationData.Save())
        Helper.Print(args.Context, "Vegetation exported to vegetation.json");
    });
  }
}
