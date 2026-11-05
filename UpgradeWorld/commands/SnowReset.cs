namespace UpgradeWorld;

public class SnowResetCommand
{
  public SnowResetCommand()
  {
    CommandWrapper.Register("snowreset", index => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("snowreset", "[...args] - Refills missing Deep North ground snow, preserving saved terrain edits. Terrain used or cached by connected players waits until it is safe.", args =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (pars.Biomes.Count > 0 && (pars.Biomes.Count != 1 || !pars.Biomes.Contains(Heightmap.Biome.DeepNorth)))
      {
        Helper.Print(args.Context, "Error: snowreset only supports DeepNorth.");
        return;
      }
      pars.Biomes.Add(Heightmap.Biome.DeepNorth);
      if (Helper.IsClient(args)) return;
      if (Executor.GetOperations().Exists(operation => operation is RefillSnow))
      {
        Helper.Print(args.Context, "A snow reset is already queued. Use start to run the queue, uw_check to inspect it, or stop to cancel it.");
        return;
      }
      Executor.AddOperation(new RefillSnow(args.Context, pars), pars.Start);
    });
  }
}
