using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class SnowResetCommand
{
  private static readonly HashSet<string> Unsupported = new(StringComparer.OrdinalIgnoreCase) { "amount", "clear", "limit", "pin", "terrain" };

  public SnowResetCommand()
  {
    var options = FiltererParameters.Parameters.Where(name => !Unsupported.Contains(name)).ToList();
    options.Add("delayed");
    var hints = FiltererParameters.GetAutoComplete();
    foreach (var name in Unsupported) hints.Remove(name);
    hints["biomes"] = _ => [nameof(Heightmap.Biome.DeepNorth)];
    hints["distance"] = _ => CommandWrapper.Info("distance=<color=yellow>min-max</color> | Distance from the center point or zone.");
    hints["noEdges"] = _ => CommandWrapper.Flag("noEdges", "Leaves snow at biome edges");
    hints["delayed"] = _ => CommandWrapper.Flag("delayed", "Waits to refill snow where connected players have cached the terrain");
    CommandWrapper.Register("resetsnow", _ => options, hints);
    Helper.Command("resetsnow", "[delayed] [...args] - Refills Deep North snow. Add delayed to wait for cached terrain. With zone, preview then use start.", args =>
    {
      var arguments = args.Args.Skip(1);
      var invalid = arguments.FirstOrDefault(arg => !ValidOption(arg));
      if (invalid != null)
      {
        Helper.Print(args.Context, $"Error: Invalid resetsnow option {invalid}. Use zone or a distance filter to limit the reset.");
        return;
      }
      FiltererParameters pars = new(args);
      if (!arguments.Any(arg => arg.StartsWith("safezones=", StringComparison.OrdinalIgnoreCase))) pars.SafeZones = 0;
      var delayed = pars.Unhandled.RemoveAll(option => IsOption(option, "delayed")) > 0;
      var explicitLive = pars.Unhandled.RemoveAll(option => IsOption(option, "live")) > 0;
      if (!pars.Valid(args.Context)) return;
      if (delayed && explicitLive)
      {
        Helper.Print(args.Context, "Choose resetsnow for a live refill or resetsnow delayed to wait for cached terrain");
        return;
      }
      if (pars.Biomes.Count > 0 && (pars.Biomes.Count != 1 || !pars.Biomes.Contains(Heightmap.Biome.DeepNorth)))
      {
        Helper.Print(args.Context, "Error: resetsnow only supports DeepNorth.");
        return;
      }
      pars.Biomes.Add(Heightmap.Biome.DeepNorth);
      if (Helper.IsClient(args)) return;
      if (Executor.DeferredSnow != null || Executor.GetOperations().Exists(operation => operation is RefillSnow))
      {
        Helper.Print(args.Context, "A snow reset is already queued. Use uw_check to see its status or stop to clear the queue.");
        return;
      }
      var snow = new RefillSnow(args.Context, pars, !delayed);
      if (pars.Zone.HasValue) Executor.AddOperation(snow, pars.Start);
      else Executor.StartSnow(snow);
    }, () => options);
  }

  private static bool ValidOption(string option)
  {
    var parts = option.Split('=');
    var name = parts[0].ToLowerInvariant();
    if (Unsupported.Contains(name) || parts.Length > 2) return false;
    if (parts.Length == 1) return true;
    var value = parts[1];
    if (name is "pos" or "zone")
    {
      var coordinates = value.Split(',');
      return coordinates.Length == 2 && (name == "pos"
        ? coordinates.All(coordinate => Finite(Parse.Float(coordinate, float.NaN)))
        : coordinates.All(coordinate => Parse.Int(coordinate, int.MinValue) is >= short.MinValue and <= short.MaxValue));
    }
    if (name == "distance")
    {
      var range = Parse.FloatRange(value, float.NaN);
      return value.Count(character => character == '-') <= 1 && NonNegative(range.Min) && NonNegative(range.Max);
    }
    if (name == "safezones") return Parse.Int(value, -1) >= 0;
    return name is not ("min" or "mindistance" or "max" or "maxdistance" or "chance") || NonNegative(Parse.Float(value, float.NaN));
  }

  private static bool IsOption(string option, string name) => option.Equals(name, StringComparison.OrdinalIgnoreCase);
  private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
  private static bool NonNegative(float value) => value >= 0 && !float.IsInfinity(value);
}
